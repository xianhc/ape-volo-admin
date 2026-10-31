<template>
  <div class="preference-panel">
    <section class="preference-section">
      <div class="preference-section-heading">
        <AppIcon icon="lucide:sun-moon" />
        <h3>{{ '界面模式' }}</h3>
      </div>
      <ThemeModeSelector
        :model-value="config.darkMode"
        :disabled="appStore.themeTransitioning"
        @update:model-value="appStore.toggleDarkMode"
      />
    </section>

    <section class="preference-section">
      <div class="preference-section-heading">
        <AppIcon icon="lucide:swatch-book" />
        <h3>{{ '主题主色' }}</h3>
      </div>
      <ThemeColorPicker
        v-model="config.primaryColor"
        @update:model-value="appStore.togglePrimaryColor"
      />
    </section>

    <section class="preference-section">
      <div class="preference-section-heading">
        <AppIcon icon="lucide:scan" />
        <h3>{{ '圆角半径' }}</h3>
      </div>
      <div class="radius-options" role="radiogroup" :aria-label="'圆角半径'">
        <button
          v-for="option in cornerSizeOptions"
          :key="option.value"
          type="button"
          role="radio"
          class="preference-choice"
          :aria-checked="config.cornerSize === option.value"
          :aria-label="option.label"
          :class="{ 'is-selected': config.cornerSize === option.value }"
          @click="appStore.toggleCornerSize(option.value)"
        >
          <span class="radius-option-face preference-preview-frame">
            <span class="radius-sample" :style="{ borderTopRightRadius: option.value + 'rem' }" aria-hidden="true"></span>
            <span>{{ option.value }}</span>
          </span>
        </button>
      </div>
    </section>

    <section class="preference-section">
      <div class="preference-section-heading">
        <AppIcon icon="lucide:type" />
        <h3>{{ '基础字号' }}</h3>
      </div>
      <div class="font-size-control">
        <el-input-number v-model="config.fontSize" :min="12" :max="20" :step="1" :aria-label="'基础字号'" />
        <span>px</span>
      </div>
      <p class="setting-help-text">{{ '调整全局基础文字和组件字号' }}</p>
      <PreferenceItem :label="'组件尺寸'" :description="'统一调整按钮、输入框和表单控件的默认尺寸'">
        <el-select v-model="config.componentSize" class="compact-select" :aria-label="'组件尺寸'" @change="appStore.toggleComponentSize">
          <el-option v-for="option in componentSizeOptions" :key="option.value" :label="option.label" :value="option.value" />
        </el-select>
      </PreferenceItem>
    </section>

    <section class="preference-section">
      <div class="preference-section-heading">
        <AppIcon icon="lucide:panel-top-dashed" />
        <h3>{{ '半暗色' }}</h3>
      </div>
      <div class="preference-group">
        <PreferenceItem :label="'暗色顶部栏'"
        >
          <el-switch v-model="config.semiDarkHeader"
          />
        </PreferenceItem>
        <PreferenceItem :label="'暗色侧边栏'"
        >
          <el-switch v-model="config.semiDarkSidebar"
          />
        </PreferenceItem>
      </div>
    </section>

    <section class="preference-section">
      <div class="preference-section-heading">
        <AppIcon icon="lucide:accessibility" />
        <h3>{{ '可读性与辅助' }}</h3>
      </div>
      <div class="preference-group">
        <PreferenceItem
          :label="'灰度模式'"
          :description="'以低饱和度方式显示页面内容'"
        >
          <el-switch v-model="config.grey" @change="appStore.toggleGrey" />
        </PreferenceItem>
        <PreferenceItem
          :label="'色弱辅助'"
          :description="'增强界面中的关键色彩对比'"
        >
          <el-switch
            v-model="config.weakness"
            @change="appStore.toggleWeakness"
          />
        </PreferenceItem>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
  /**
   * 外观设置页。
   */
  import AppIcon from '@/components/AppIcon.vue'
  import { computed } from 'vue'
  import { storeToRefs } from 'pinia'
  import { useAppStore } from '@/pinia'
  import { BorderRadiusType } from '@/enums/BorderRadiusType'
  import { ComponentSizeType } from '@/enums/ComponentSizeType'
  import ThemeModeSelector from '../../components/ThemeModeSelector.vue'
  import ThemeColorPicker from '../../components/ThemeColorPicker.vue'
  import PreferenceItem from '../../components/PreferenceItem.vue'
  defineOptions({
    name: 'AppearanceSettings'
  })

  const appStore = useAppStore()
  const { config } = storeToRefs(appStore)

  /**
   * 生成界面控件尺寸的本地化候选项。
   * @returns 组件尺寸枚举与当前语言文案组成的候选项。
   */
  const buildComponentSizeOptions = () => {
    const labelMap: Record<string, string> = {
      [ComponentSizeType.Default]: '默认尺寸',
      [ComponentSizeType.Large]: '宽松尺寸',
      [ComponentSizeType.Small]: '紧凑尺寸'
    }

    return (Object.values(ComponentSizeType) as string[]).map((value) => ({
      value,
      label: labelMap[value] ?? value
    }))
  }

  /**
   * 生成界面圆角尺寸的本地化候选项。
   * @returns 圆角尺寸枚举与当前语言文案组成的候选项。
   */
  const buildCornerSizeOptions = () => {
    const labelMap: Partial<Record<BorderRadiusType, string>> = {
      [BorderRadiusType.None]: '无圆角',
      [BorderRadiusType.Small]: '小圆角',
      [BorderRadiusType.Medium]: '中圆角',
      [BorderRadiusType.Large]: '大圆角',
      [BorderRadiusType.Full]: '超大圆角'
    }

    return (
      Object.values(BorderRadiusType).filter(
        (v): v is number => typeof v === 'number'
      ) as BorderRadiusType[]
    ).map((value) => ({
      value,
      label: labelMap[value] ?? String(value)
    }))
  }

  const componentSizeOptions = computed(buildComponentSizeOptions)
  const cornerSizeOptions = computed(buildCornerSizeOptions)
</script>

<style scoped>
  .radius-options {
    display: grid;
    grid-template-columns: repeat(5, minmax(0, 1fr));
    gap: 10px;
  }
  .radius-options button {
    min-width: 0;
    padding: 0;
    border: 0;
    border-radius: var(--preference-radius, 12px);
    background: transparent;
    color: var(--app-text-secondary);
    font: inherit;
    font-size: 13px;
    cursor: pointer;
  }
  .radius-options button.is-selected {
    color: var(--el-color-primary);
  }
  .radius-option-face {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    height: 44px;
    border: 1px solid var(--app-border);
    border-radius: var(--preference-radius, 12px);
  }
  .radius-sample {
    flex: none;
    width: 14px;
    height: 14px;
    border-top: 2px solid currentColor;
    border-right: 2px solid currentColor;
  }
  .font-size-control {
    display: flex;
    align-items: center;
    gap: 12px;
    color: var(--app-text-muted);
    font-size: 13px;
  }
  .font-size-control :deep(.el-input-number) {
    flex: 1;
    width: auto;
    height: 42px;
  }
  .font-size-control :deep(.el-input__wrapper) {
    height: 42px;
  }
  .setting-help-text {
    margin: -4px 0 0;
  }
  @media (max-width: 400px) {
    .radius-options {
      gap: 6px;
    }
    .radius-options button {
      font-size: 12px;
    }
    .radius-option-face {
      gap: 5px;
    }
    .radius-sample {
      width: 10px;
      height: 10px;
    }
  }
</style>
