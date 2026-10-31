<template>
  <el-form
    :model="model"
    label-position="right"
    label-width="auto"
    class="search-panel"
    @submit.prevent="emit('query')"
  >
    <div class="search-panel__grid">
      <div class="search-panel__fields">
        <slot />
        <!-- 高级条件保存在父级模型；折叠时不挂载字段，避免 auto 标签测量隐藏元素得到 NaN。 -->
        <div v-if="hasAdvanced" :id="advancedId" class="search-panel__advanced">
          <slot v-if="expanded" name="advanced" />
        </div>
      </div>
      <div class="search-panel__actions">
        <el-button :disabled="loading" @click="emit('reset')">
          {{ '重置' }}
        </el-button>
        <el-button type="primary" :loading="pending" native-type="submit">
          {{ '查询' }}
        </el-button>
        <el-button
          v-if="hasAdvanced"
          type="primary"
          link
          :class="{ 'has-active-filters': advancedActive }"
          :aria-expanded="expanded"
          :aria-controls="advancedId"
          @click="expanded = !expanded"
        >
          {{ (expanded ? '收起' : '展开') }}
          <el-icon
            class="search-panel__toggle"
            :class="{ 'is-expanded': expanded }"
          >
            <AppIcon icon="lucide:chevron-down" />
          </el-icon>
        </el-button>
      </div>
    </div>
  </el-form>
</template>

<script setup lang="ts">
  /**
   * 表格搜索面板组件。
   */
  import AppIcon from '@/components/AppIcon.vue'
  import { computed, inject, useId, useSlots } from 'vue'
  import { tableLoadingKey } from './tableContext'
  defineOptions({ name: 'SearchPanel' })

  const props = withDefaults(
    defineProps<{
      /** 当前界面的编辑模型。 */
      model: object
      /** 是否正在加载数据。 */
      loading?: boolean
      /** 高级筛选是否存在有效条件。 */
      advancedActive?: boolean
    }>(),
    {
      loading: false,
      advancedActive: false
    }
  )
  const tableLoading = inject(tableLoadingKey, null)
  /**
   * 查询按钮的等待状态。
   *
   * 位于表格内时跟随表格的「再次加载」语义：首次进入由表格遮罩反馈，
   * 这里不再重复转圈；独立使用时仍按传入的 loading 展示。
   */
  const pending = computed(() =>
    tableLoading ? tableLoading.refreshing.value : props.loading
  )

  const emit = defineEmits<{
    /** 当前查询的筛选条件。 */
    query: []
    /** 通知父组件恢复默认列或筛选配置。 */
    reset: []
  }>()
  const expanded = defineModel<boolean>('expanded', { default: false })
  const advancedId = `search-advanced-${useId()}`
  const slots = useSlots()
  const hasAdvanced = computed(() => Boolean(slots.advanced))
</script>

<style scoped>
  .search-panel {
    min-width: 0;
  }

  .search-panel__grid {
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    align-items: center;
    gap: var(--app-space-3) var(--app-space-4);
  }

  .search-panel__fields,
  .search-panel__advanced {
    display: contents;
  }

  .search-panel__actions {
    display: flex;
    align-items: center;
    justify-content: flex-end;
    grid-column: -2 / -1;
    gap: var(--app-space-2);
  }

  .search-panel__actions :deep(.el-button + .el-button) {
    margin-left: 0;
  }

  .search-panel__fields :deep(.el-form-item),
  .search-panel__advanced :deep(.el-form-item) {
    min-width: 0;
    margin: 0;
  }

  .search-panel__fields :deep(.el-form-item__content),
  .search-panel__advanced :deep(.el-form-item__content) {
    min-width: 0;
    flex: 1;
    flex-wrap: nowrap;
  }

  .search-panel :deep(.el-form-item__label) {
    flex-shrink: 0;
    white-space: nowrap;
  }

  .search-panel :deep(.el-input),
  .search-panel :deep(.el-select),
  .search-panel :deep(.el-cascader),
  .search-panel :deep(.el-date-editor),
  .search-panel :deep(.el-autocomplete) {
    width: 100% !important;
  }

  .search-panel__toggle {
    margin-left: var(--app-space-1);
    transition: transform var(--el-transition-duration-fast);
  }

  .search-panel__toggle.is-expanded {
    transform: rotate(180deg);
  }

  .has-active-filters {
    font-weight: 600;
  }

  @media (max-width: 1199px) {
    .search-panel__grid {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }
  }

  @media (max-width: 767px) {
    .search-panel__grid {
      grid-template-columns: minmax(0, 1fr);
    }

    .search-panel__actions {
      justify-content: flex-start;
    }

    .search-panel :deep(.el-form-item) {
      display: block;
    }

    .search-panel :deep(.el-form-item__label) {
      display: block;
      height: auto;
      margin-bottom: 6px;
      padding: 0;
      line-height: 22px;
    }

    .search-panel :deep(.el-form-item__label-wrap) {
      margin-left: 0 !important;
    }
  }

  @media (prefers-reduced-motion: reduce) {
    .search-panel__toggle {
      transition: none;
    }
  }
</style>
