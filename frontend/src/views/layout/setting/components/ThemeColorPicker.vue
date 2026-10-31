<template>
  <div class="theme-color-panel">
    <div
      class="theme-color-options"
      role="radiogroup"
      :aria-label="'推荐配色'"
    >
      <button
        v-for="colorItem in presetColors"
        :key="colorItem.value"
        type="button"
        class="theme-color-option preference-choice"
        :class="{ 'is-selected': modelValue.toLowerCase() === colorItem.value.toLowerCase() }"
        role="radio"
        :aria-checked="modelValue.toLowerCase() === colorItem.value.toLowerCase()"
        :aria-label="colorItem.label"
        :title="colorItem.label"
        @click="handleColorChange(colorItem.value)"
      >
        <span class="theme-color-face preference-preview-frame">
          <span
            class="theme-color-swatch"
            :style="{ backgroundColor: colorItem.value }"
          >
            <AppIcon
              v-if="modelValue.toLowerCase() === colorItem.value.toLowerCase()"
              icon="lucide:check"
              :style="{ color: checkColor(colorItem.value) }"
            />
          </span>
        </span>
        <span class="theme-color-label">{{ colorItem.label }}</span>
      </button>
      <div class="custom-color-option" :class="{ 'is-selected': isCustomColor }">
        <el-color-picker
          v-model="customColor"
          :predefine="presetColors.map((item) => item.value)"
          :aria-label="'自定义主色'"
          class="custom-color-picker"
          @change="handleCustomColorChange"
        />
        <span class="theme-color-label">{{ '自定义主色' }}</span>
      </div>
    </div>
    <code class="custom-theme-color-code">{{ modelValue }}</code>
  </div>
</template>

<script setup lang="ts">
  /**
   * 主题色选择组件。
   */
  import AppIcon from '@/components/AppIcon.vue'
  import { computed, ref, watch } from 'vue'
  import { DEFAULT_PRIMARY_COLOR, getContrastRatio } from '@/services/appearance/themeRuntime'
  import { THEME_COLOR_PRESETS } from '@/services/appearance/themePalette'
  defineOptions({
    name: 'ThemeColorPicker'
  })

  const props = withDefaults(
    defineProps<{
      /** 由父组件持有的受控绑定值。 */
      modelValue?: string
    }>(),
    {
      modelValue: DEFAULT_PRIMARY_COLOR
    }
  )

  const emit = defineEmits<{
    /** 将新的绑定值通知给父组件。 */
    'update:modelValue': [color: string]
  }>()

  const customColor = ref<string>(props.modelValue)

  /** 与登录页共用的推荐主色，标签随当前语言更新。 */
  const presetColors = computed(() => THEME_COLOR_PRESETS.map(preset => ({
    value: preset.value,
    label: preset.label
  })))

  /** 未命中共用色板的主色由自定义入口继续展示，不限制为预设值。 */
  const isCustomColor = computed(() => !presetColors.value.some(item => item.value.toLowerCase() === props.modelValue.toLowerCase()))

  /**
   * 根据背景色对比度选择可读的前景文字颜色。
   * @param color 待校验或应用的颜色值。
   * @returns 与给定背景色具有足够对比度的白色或深色前景。
   */
  const checkColor = (color: string): string =>
    getContrastRatio('#FFFFFF', color) >= 3 ? '#FFFFFF' : '#1F292E'

  /**
   * 将选中的颜色或徽标类型写入当前配置。
   * @param color 待校验或应用的颜色值。
   */
  const handleColorChange = (color: string): void => {
    customColor.value = color
    emit('update:modelValue', color)
  }

  /**
   * 将选中的颜色或徽标类型写入当前配置。
   * @param color 待校验或应用的颜色值。
   */
  const handleCustomColorChange = (color: string | null): void => {
    if (color) {
      emit('update:modelValue', color)
    }
  }

  watch(
    () => props.modelValue,
    (newValue) => {
      customColor.value = newValue
    }
  )
</script>

<style scoped>
  .theme-color-options {
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 16px 18px;
  }
  .theme-color-option {
    display: grid;
    gap: 8px;
    min-width: 0;
    padding: 0;
    border: 0;
    background: transparent;
    color: var(--app-text-secondary);
    font: inherit;
    cursor: pointer;
  }
  .theme-color-face {
    display: grid;
    place-items: center;
    height: 58px;
    border: 1px solid var(--app-border);
    border-radius: var(--preference-radius, 10px);
    transition: border-color 180ms, box-shadow 180ms;
  }
  .theme-color-option:hover .theme-color-face {
    border-color: var(--el-color-primary);
  }
  .theme-color-option.is-selected .theme-color-face {
    border-color: var(--el-color-primary);
    box-shadow: 0 0 0 1px var(--el-color-primary);
  }
  .theme-color-option:focus-visible {
    outline: 2px solid var(--el-color-primary);
    outline-offset: 4px;
    border-radius: 10px;
  }
  .theme-color-swatch {
    display: grid;
    place-items: center;
    width: 26px;
    height: 26px;
    border-radius: 7px;
    box-shadow: inset 0 0 0 1px rgb(0 0 0 / 8%);
    color: white;
    transition: transform 200ms;
  }
  .theme-color-option:hover .theme-color-swatch {
    transform: scale(1.12);
  }
  .theme-color-label {
    font-size: 12px;
    line-height: 1.5;
    text-align: center;
  }
  .custom-color-option {
    display: grid;
    gap: 8px;
    color: var(--app-text-secondary);
  }
  .custom-color-option :deep(.custom-color-picker) {
    width: 100%;
    height: 58px;
  }
  .custom-color-option :deep(.el-color-picker__trigger) {
    width: 100%;
    height: 58px;
    border-color: var(--app-border);
    border-radius: var(--preference-radius, 10px);
    padding: 15px;
  }
  .custom-color-option :deep(.el-color-picker__color) {
    width: 26px;
    margin: auto;
    border: 0;
    border-radius: 7px;
    overflow: hidden;
  }
  .custom-color-option.is-selected :deep(.el-color-picker__trigger) {
    border-color: var(--el-color-primary);
    box-shadow: 0 0 0 1px var(--el-color-primary);
  }
  .custom-theme-color-code {
    display: block;
    margin-top: 12px;
    color: var(--app-text-muted);
    font-family: inherit;
    font-size: 12px;
    text-align: right;
  }
</style>
