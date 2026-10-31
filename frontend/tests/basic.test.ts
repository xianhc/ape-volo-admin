import assert from 'node:assert/strict'
import test from 'node:test'
import { buildDepartmentTree } from '../src/api/permission/departmentAdapter'
import { getMenuNavigationTarget } from '../src/services/navigation/menuNavigation'
import { MenuType } from '../src/enums/MenuType'

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

test('externalMenuAllowsHttpUrlsAndRejectsUnsafeSchemes', () => {
  for (const component of ['javascript:void 0', 'data:text/html,test', '//example.com']) {
    assert.equal(getMenuNavigationTarget({ menuType: MenuType.ExternalLink, component }), null)
  }
  assert.equal(getMenuNavigationTarget({ menuType: MenuType.ExternalLink, component: 'https://example.com/docs' }), 'https://example.com/docs')
})
