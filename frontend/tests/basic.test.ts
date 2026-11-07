import assert from 'node:assert/strict'
import test from 'node:test'
import { buildDepartmentTree, filterDepartmentTree } from '../src/api/organization/department/adapter'
import { getMenuNavigationTarget } from '../src/services/navigation/menuNavigation'
import { MenuType } from '../src/enums/MenuType'
import { createUserDraft, toUserRequest } from '../src/api/identity/userAdapter'
import { toRoleRequest } from '../src/api/identity/roleAdapter'
import { toMenuRequest } from '../src/api/identity/menuAdapter'
import { isSessionIdentityChange } from '../src/services/session/tokenStorage'
import type { ApeToken } from '../src/services/session/tokenStorage'
import { AxiosError, AxiosHeaders } from 'axios'
import { createHttpClient } from '../src/services/http/client'
import { createTokenRefresher } from '../src/services/session/tokenRefresh'

test('refreshRejectionEndsSessionWhileTemporaryFailuresKeepCredentials', async () => {
  for (const proactive of [true, false]) {
    for (const status of [400, 401, 403, 429, 500, undefined]) {
      let token: ApeToken = { sessionId: 'first', accessToken: 'access', refreshToken: 'refresh',
        tokenType: 'Bearer', expires: proactive ? 0 : 2000, refreshTokenExpires: 3000 }
      let expired = 0
      let errors = 0
      let requests = 0
      const refresh = createTokenRefresher({
        read: () => token,
        save: () => assert.fail('Rejected refresh must not save credentials'),
        request: async () => {
          throw new AxiosError('Refresh failed', undefined, undefined, undefined, status === undefined ? undefined : {
            status, statusText: 'Rejected', headers: {}, config: { headers: new AxiosHeaders() },
            data: { isSuccess: false, status, message: 'token验证失败，请重新登录！' }
          })
        },
        withLock: work => work(), now: () => 1000
      })
      const client = createHttpClient({
        config: { adapter: async config => {
          requests++
          throw new AxiosError('Unauthorized', undefined, config, undefined, {
            status: 401, statusText: 'Unauthorized', data: {}, headers: {}, config
          })
        } },
        session: { read: () => token, refresh, invalidate: () => {
          token = { ...token, sessionId: '', accessToken: '', refreshToken: '' }
        } },
        feedback: { expired: () => { expired++ }, error: () => { errors++ }, forbidden: () => assert.fail('Refresh must use session expiry') },
        now: () => 1000
      })
      await assert.rejects(client.get('/users', { errorFeedback: false }))
      const rejected = status === 400 || status === 401 || status === 403
      assert.equal(expired, rejected ? 1 : 0, `proactive=${proactive}, status=${status}`)
      assert.equal(token.sessionId, rejected ? '' : 'first')
      assert.equal(errors, 0)
      assert.equal(requests, proactive ? 0 : 1)
    }
  }
})

test('crossTabSessionChangesClearPagesWhileTokenRotationKeepsThem', () => {
  const key = 'ape-volo-admin-token'
  const oldValue = JSON.stringify({ sessionId: 'first', accessToken: 'old' })
  assert.equal(isSessionIdentityChange({ key, oldValue, newValue: JSON.stringify({ sessionId: 'second' }) }), true)
  assert.equal(isSessionIdentityChange({ key, oldValue, newValue: null }), true)
  assert.equal(isSessionIdentityChange({ key, oldValue: null, newValue: oldValue }), true)
  assert.equal(isSessionIdentityChange({ key, oldValue, newValue: JSON.stringify({ sessionId: 'first', accessToken: 'new' }) }), false)
  assert.equal(isSessionIdentityChange({ key: 'preferences', oldValue, newValue: null }), false)
  assert.equal(isSessionIdentityChange({ key: null, oldValue: null, newValue: null }), true)
  assert.equal(isSessionIdentityChange({ key, oldValue, newValue: 'invalid' }), true)
})

test('departmentTreePreservesHierarchyAndSource', () => {
  const departments = [
    { id: '9007199254740993', parentId: '1', name: 'Child', creationTime: '' },
    { id: '1', parentId: null, name: 'Root', creationTime: '' },
    { id: '3', parentId: '9007199254740993', name: 'Grandchild', creationTime: '' }
  ]
  const original = structuredClone(departments)
  const tree = buildDepartmentTree(departments)
  assert.equal(tree.length, 1)
  assert.equal(tree[0].id, '1')
  assert.equal(tree[0].children![0].id, '9007199254740993')
  assert.equal(tree[0].children![0].children![0].id, '3')
  assert.deepEqual(departments, original)
})

test('departmentFiltersFindNestedMatchesAndPreserveAncestorsAndSource', () => {
  const tree = buildDepartmentTree([
    { id: '1', parentId: null, name: 'Head Office', enabled: false, creationTime: '2025-01-01 00:00:00' },
    { id: '9007199254740993', parentId: '1', name: 'Engineering', enabled: true, creationTime: '2026-10-01 00:00:00' },
    { id: '3', parentId: '1', name: 'Sales', enabled: true, creationTime: '2025-01-01 00:00:00' }
  ])
  const original = structuredClone(tree)
  const result = filterDepartmentTree(tree, {
    name: ' ENGINEERING ', enabled: true,
    startTime: '2026-01-01 00:00:00', endTime: '2026-11-01 00:00:00'
  })
  assert.equal(result[0].id, '1')
  assert.deepEqual(result[0].children?.map(node => node.id), ['9007199254740993'])
  assert.deepEqual(filterDepartmentTree(tree, { name: 'Missing' }), [])
  assert.deepEqual(filterDepartmentTree(tree, { enabled: false })[0].children, [])
  assert.deepEqual(tree, original)
})

test('departmentSortingKeepsParentChildRelationshipsAndLongIds', () => {
  const tree = buildDepartmentTree([
    { id: '1', parentId: null, name: 'Root', sort: 2, creationTime: '' },
    { id: '2', parentId: null, name: 'Second', sort: 1, creationTime: '' },
    { id: '9007199254740993', parentId: '1', name: 'Later', sort: 2, creationTime: '' },
    { id: '9007199254740992', parentId: '1', name: 'Earlier', sort: 1, creationTime: '' }
  ])
  const ascending = filterDepartmentTree(tree, { sorting: 'sort asc' })
  assert.deepEqual(ascending.map(node => node.id), ['2', '1'])
  assert.deepEqual(ascending[1].children?.map(node => node.id), ['9007199254740992', '9007199254740993'])
  assert.deepEqual(filterDepartmentTree(tree, { sorting: 'id desc' })[1].children?.map(node => node.id), ['9007199254740993', '9007199254740992'])
})

test('externalMenuAllowsHttpUrlsAndRejectsUnsafeSchemes', () => {
  for (const component of ['javascript:void 0', 'data:text/html,test', '//example.com']) {
    assert.equal(getMenuNavigationTarget({ menuType: MenuType.ExternalLink, component }), null)
  }
  assert.equal(getMenuNavigationTarget({ menuType: MenuType.ExternalLink, component: 'https://example.com/docs' }), 'https://example.com/docs')
})

test('userWritePayloadExcludesAuditFieldsAndCopiesRelations', () => {
  const draft = { ...createUserDraft(), id: '9007199254740993', dept: { id: '2', name: 'Engineering' },
    roles: [{ id: '3', name: 'Operator' }], positions: [{ id: '4', name: 'Engineer' }], creatorId: '999', isDeleted: true }
  const request = toUserRequest(draft)
  assert.equal(request.id, '9007199254740993')
  assert.deepEqual(request.dept, { id: '2' })
  assert.equal('creatorId' in request, false)
  assert.equal('isDeleted' in request, false)
  request.roles[0].id = '5'
  assert.equal(draft.roles[0].id, '3')
  assert.throws(() => toUserRequest(createUserDraft()), /请选择部门/)
})

test('roleWritePayloadOnlySubmitsCustomDepartmentIds', () => {
  const draft = { id: '1', name: 'Operator', authCode: 'operator', dataScopeType: 5, deptIdArray: ['2'],
    menuList: [{ id: '999' }], creatorId: '888' }
  const request = toRoleRequest(draft)
  assert.deepEqual(request.deptIdArray, ['2'])
  assert.notEqual(request.deptIdArray, draft.deptIdArray)
  assert.equal('menuList' in request, false)
  assert.equal('creatorId' in request, false)
  assert.deepEqual(toRoleRequest({ ...draft, dataScopeType: 2 }).deptIdArray, [])
})

test('linkMenuPayloadCannotCarryInterfacePermissionCodes', () => {
  const draft = { id: '1', title: 'Link', menuType: MenuType.ExternalLink, sort: 1, parentId: null,
    keepAlive: false, enabled: true, hidden: false, authCode: 'sys:user:del', creatorId: '999', children: [] }
  const request = toMenuRequest(draft)
  assert.equal(request.authCode, undefined)
  assert.equal('creatorId' in request, false)
  assert.equal('children' in request, false)
  assert.equal(toMenuRequest({ ...draft, menuType: MenuType.Button }).authCode, 'sys:user:del')
})
