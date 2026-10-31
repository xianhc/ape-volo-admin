<template>
  <div class="ape-theme-setting-item">
    <div class="preference-copy">
      <span class="ape-theme-setting-label">{{ label }}</span>
      <span v-if="description" class="setting-help-text">{{
        description
      }}</span>
      <slot v-else name="suffix"></slot>
    </div>
    <div class="flex items-center setting-controls">
      <slot></slot>
    </div>
  </div>
</template>

<script setup lang="ts">
  /**
   * 偏好设置项组件。
   */
  defineOptions({
    name: 'PreferenceItem'
  })

  defineProps<{
    /** 面向用户显示的标签。 */
    label: string
    /** 用于解释用途的描述。 */
    description?: string
  }>()
</script>

<style scoped>
  .preference-copy {
    display: grid;
    flex: 1;
    gap: 2px;
    min-width: 0;
    padding-right: 0;
  }

  .setting-controls {
    flex: none;
    min-width: 0;

    ::v-deep(.el-switch) {
      --el-switch-on-color: var(--el-color-primary);
      --el-switch-off-color: var(--app-border-strong);
    }

    ::v-deep(.el-select),
    ::v-deep(.el-input-number) {
      width: 100%;
    }
  }
</style>
