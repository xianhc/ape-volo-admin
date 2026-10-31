<template>
  <el-drawer
    v-if="dialogFormVisible"
    v-model="dialogFormVisible"
    :size="appStore.drawerSize"
    title="权限配置"
  >
    <div class="authorize-pane">
      <div class="authorize-toolbar">
        <el-input
          v-model="filterMenuText"
          class="w-3/5"
          placeholder="请输入关键字筛选"
        />
        <el-button type="primary" @click="saveMenuTree">确 定</el-button>
      </div>
      <div class="authorize-scroll">
        <el-tree
          ref="menuTree"
          :data="menuTreeData"
          :default-checked-keys="menuTreeIds"
          :props="{
            children: 'children',
            label: 'title',
            isLeaf: 'leaf'
          }"
          default-expand-all
          highlight-current
          node-key="id"
          show-checkbox
          check-strictly
          :filter-node-method="filterMenuNode"
        >
        </el-tree>
      </div>
    </div>
  </el-drawer>
</template>

<script setup lang="ts">
  import { getAll as getAllMenu, type Menu } from '@/api/permission/menu'
  import { single, editRoleMenu } from '@/api/permission/role'
  import { nextTick, ref, watch } from 'vue'
  import { ElMessage } from 'element-plus'
  import { useAppStore } from '@/pinia'
  import type { ElTree } from 'element-plus'

  defineOptions({
    name: 'Authorize'
  })

  const appStore = useAppStore()

  const roleId = ref('')

  const filterMenuText = ref('')
  const menuTreeData = ref<Menu[]>([])
  const menuTreeIds = ref<string[]>([])

  const init = async () => {
    const resRole = await single({ id: roleId.value })
    const resMenu = await getAllMenu()
    menuTreeData.value = resMenu.data
    const roleMenuList = resRole.data.menuList || []
    const menuIdArr: string[] = []
    roleMenuList.forEach((item: any) => {
      menuIdArr.push(String(item.id))
    })
    menuTreeIds.value = menuIdArr

    // 注意：等 nextTick 后再设置选中
    await nextTick(() => {
      if (menuTree.value && menuIdArr.length) {
        menuTree.value.setCheckedKeys(menuIdArr)
      }
    })
  }

  const menuTree = ref<InstanceType<typeof ElTree> | null>(null)
  const saveMenuTree = async () => {
    const checkArr = menuTree.value?.getCheckedNodes(false, false) || []
    await editRoleMenu({
      id: roleId.value,
      menuIdArray: checkArr.map((item: any) => item.id)
    })
    ElMessage({
      type: 'success',
      message: '角色菜单设置成功!'
    })
  }

  const filterMenuNode = (value: string, data: any) => {
    if (!value) return true
    return data.title.indexOf(value) !== -1
  }
  watch(filterMenuText, (val) => {
    menuTree.value?.filter(val)
  })

  const dialogFormVisible = ref(false)
  const openAuthorizeDialog = async (id: string | number) => {
    roleId.value = String(id)
    dialogFormVisible.value = true
    await init() // 初始化数据
  }

  defineExpose({ openAuthorizeDialog })
</script>

<style lang="scss" scoped>
  .custom-tree-node {
    span + span {
      @apply ml-3;
    }
  }

  // 抽屉正文本身不滚动：整块改成 flex 列，只有下方树区域可滚
  :deep(.el-drawer__body) {
    display: flex;
    flex-direction: column;
    overflow: hidden;
  }

  .authorize-pane {
    display: flex;
    flex: 1;
    flex-direction: column;
    min-height: 0;
  }

  // 筛选框 + 确定按钮：固定不滚
  .authorize-toolbar {
    display: flex;
    flex: none;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    padding-bottom: 12px;
  }

  // 只有这里滚动
  .authorize-scroll {
    flex: 1;
    min-height: 0;
    overflow: auto;
  }
</style>
