<template>
  <el-drawer
    v-model="dialogVisible"
    :size="appStore.drawerSize"
    :before-close="closeDialog"
    :show-close="false"
  >
    <template #header>
      <div class="flex justify-between items-center">
        <span class="text-lg">{{ getFormTitle() }}</span>
        <div>
          <el-button @click="closeDialog">取 消</el-button>
          <el-button type="primary" :loading="loading" @click="handleSave">
            确 定
          </el-button>
        </div>
      </div>
    </template>
    <el-form
      ref="menuForm"
      :model="form"
      :rules="dynamicRules"
      :validate-on-rule-change="false"
      :inline="true"
      label-width="120px"
      label-position="right"
      class="grid grid-cols-2"
    >
      <el-form-item label="菜单类型" class="col-span-2" prop="menuType">
        <el-radio-group v-model="form.menuType">
          <el-radio-button
            v-for="item in props.menuTypeOption"
            :key="item.id"
            v-model="form.menuType"
            :label="Number(item.value)"
          >
            {{ item.label }}
          </el-radio-button>
        </el-radio-group>
      </el-form-item>
      <el-form-item :label="titleLabel" prop="title">
        <el-input v-model="form.title" />
      </el-form-item>
      <el-form-item label="上级菜单">
        <el-cascader
          v-model="form.parentId"
          style="width: 100%"
          :options="menuOption"
          :disabled="true"
          :props="{
            checkStrictly: true,
            label: 'title',
            value: 'id',
            emitPath: false
          }"
          :show-all-levels="false"
          filterable
        />
      </el-form-item>
      <el-form-item
        v-if="form.menuType !== MenuType.Button"
        :label="componentLabel"
        prop="component"
      >
        <el-input v-model="form.component" />
      </el-form-item>
      <el-form-item
        v-if="[MenuType.Catalog, MenuType.Menu].includes(form.menuType)"
        label="组件名称"
        prop="componentName"
      >
        <el-input v-model="form.componentName" />
      </el-form-item>
      <el-form-item
        v-if="form.menuType !== MenuType.Button"
        label="路由地址"
        prop="path"
      >
        <el-input v-model="form.path" />
      </el-form-item>
      <el-form-item
        v-if="form.menuType !== MenuType.Button"
        label="图标"
        prop="icon"
      >
        <IconPicker v-model="form.icon" />
      </el-form-item>
      <el-form-item
        v-if="form.menuType !== MenuType.Button"
        label="是否隐藏"
        prop="hidden"
      >
        <el-radio-group v-model="form.hidden">
          <el-radio border :value="true">是</el-radio>
          <el-radio border :value="false">否</el-radio>
        </el-radio-group>
      </el-form-item>
      <el-form-item
        v-if="form.menuType !== MenuType.Button"
        label="是否缓存"
        prop="keepAlive"
      >
        <el-radio-group v-model="form.keepAlive">
          <el-radio border :value="true">是</el-radio>
          <el-radio border :value="false">否</el-radio>
        </el-radio-group>
      </el-form-item>
      <el-form-item
        v-if="[MenuType.Menu, MenuType.Button].includes(form.menuType)"
        label="权限标识"
        prop="authCode"
      >
        <el-input v-model="form.authCode" />
      </el-form-item>

      <el-form-item label="状态" prop="enabled">
        <el-radio-group v-model="form.enabled">
          <el-radio
            v-for="item in props.statusTypeOption"
            :key="item.id"
            border
            :value="strToBool(String(item.value))"
          >
            {{ item.label }}
          </el-radio>
        </el-radio-group>
      </el-form-item>
      <el-form-item label="排序" prop="sort">
        <el-input-number
          v-model.number="form.sort"
          :min="0"
          :max="999"
          controls-position="right"
          style="width: 230px"
        />
      </el-form-item>
    </el-form>
  </el-drawer>
</template>
<script setup lang="ts">
  import type { FormInstance } from 'element-plus'
  import { useCrudForm } from '@/components/crud/crudContext'
  import type { EntityDraft } from '@/api/types/base'
  import type { Menu } from '@/api/permission/types/menu.types'

  import { useAppStore } from '@/pinia'
  import IconPicker from '@/components/IconPicker.vue'
  import { computed, ref } from 'vue'
  import { strToBool } from '@/utils/converter'
  import { getAll } from '@/api/permission/menu'
  import { MenuType } from '@/enums/MenuType'
  import type { DictOption } from '@/composables/useDictionary'

  const appStore = useAppStore()

  const props = defineProps<{
    statusTypeOption: DictOption[]
    menuTypeOption: DictOption[]
  }>()

  const menuOption = ref([
    {
      id: '0',
      title: '顶级菜单'
    }
  ])

  const setMenuOptions = (menuData: any, optionsData: any) => {
    menuData &&
      menuData.forEach((item: any) => {
        if (item.children && item.children.length) {
          const option = {
            title: item.title,
            id: item.id,
            children: []
          }
          setMenuOptions(item.children, option.children)
          optionsData.push(option)
        } else {
          const option = {
            title: item.title,
            id: item.id
          }
          optionsData.push(option)
        }
      })
  }

  const init = async () => {
    const res = await getAll()
    setMenuOptions(res.data, menuOption.value)
  }
  init()

  const dynamicRules = computed(() => {
    const baseRules: Record<string, any> = {
      title: [
        {
          required: true,
          message: `请输入${titleLabel.value}`,
          trigger: 'blur'
        }
      ],
      sort: [{ required: true, message: '请填写排序序号', trigger: 'blur' }],
      enabled: [{ required: true, message: '请选择', trigger: 'blur' }]
    }
    if (form.value.menuType !== MenuType.Button) {
      baseRules.hidden = [
        { required: true, message: '请选择', trigger: 'blur' }
      ]
      baseRules.keepAlive = [
        { required: true, message: '请选择', trigger: 'blur' }
      ]
      baseRules.component = [
        {
          required: true,
          message: `请输入${componentLabel.value}`,
          trigger: 'blur'
        }
      ]
      if (
        ![MenuType.InternalLink, MenuType.ExternalLink].includes(
          form.value.menuType
        )
      ) {
        baseRules.componentName = [
          { required: true, message: '请输入组件名称', trigger: 'blur' }
        ]
      }
    }
    if ([MenuType.Menu, MenuType.Button].includes(form.value.menuType)) {
      baseRules.authCode = [
        { required: true, message: '请输入权限标识', trigger: 'blur' }
      ]
    }

    return baseRules
  })

  const titleLabel = computed(() => {
    if (form.value.menuType === MenuType.Button) return '按钮名称'
    return '菜单标题'
  })

  const componentLabel = computed(() =>
    [MenuType.InternalLink, MenuType.ExternalLink].includes(form.value.menuType)
      ? '链接地址'
      : '页面组件'
  )

  // 注入crud
  const crud = useCrudForm<EntityDraft<Menu> & { menuType: number }>()
  const {
    form,
    dialogVisible,
    loading,
    closeDialog,
    validateAndSave,
    getFormTitle
  } = crud

  const menuForm = ref<FormInstance>()
  // 验证表单并保存
  const handleSave = () => validateAndSave(menuForm.value)
</script>
