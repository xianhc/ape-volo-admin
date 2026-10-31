<script setup lang="ts">
  import { computed, shallowRef } from 'vue'
  import { onClickOutside } from '@vueuse/core'
  import AppIcon from '@/components/AppIcon.vue'
  import { useAppStore } from '@/pinia'
  import { getContrastRatio } from '@/services/appearance/themeRuntime'
  import { THEME_COLOR_PRESETS } from '@/services/appearance/themePalette'

  const appStore = useAppStore()
  /** 当前是否展开横向色板；该临时状态不进入全局偏好。 */
  const isPaletteOpen = shallowRef(false)
  /** 色板和触发器的共同容器，用于判断外部点击。 */
  const paletteHost = shallowRef<HTMLElement | null>(null)
  /** 通过 Escape 关闭色板后恢复键盘焦点的按钮。 */
  const paletteTrigger = shallowRef<HTMLButtonElement | null>(null)
  /** 预设之外的颜色继续展示为当前自定义色，不将其强制替换。 */
  const isCustomColor = computed(() => !THEME_COLOR_PRESETS.some(preset => preset.value.toLowerCase() === appStore.config.primaryColor.toLowerCase()))

  /** 收起临时色板，不改变用户的全局主题选择。 */
  function closePalette(): void {
    isPaletteOpen.value = false
  }

  /** 用 Escape 收起色板，并将焦点返回触发器。 */
  function closePaletteWithFocus(): void {
    if (!isPaletteOpen.value) return
    closePalette()
    paletteTrigger.value?.focus()
  }

  /**
   * 为色板勾选标记选择足够清楚的前景色。
   * @param color 当前色块的背景色。
   * @returns 与色块对比度合适的白色或深色。
   */
  function checkColor(color: string): string {
    return getContrastRatio('#ffffff', color) >= 3 ? '#ffffff' : '#1f292e'
  }

  onClickOutside(paletteHost, closePalette)
</script>

<template>
  <div class="appearance-tools" @keydown.esc.stop.prevent="closePaletteWithFocus">
    <div ref="paletteHost" class="palette-host" :class="{ 'is-open': isPaletteOpen }">
      <div class="swatches-wrap" :aria-hidden="!isPaletteOpen" :inert="!isPaletteOpen || undefined">
        <div class="swatches" role="group" aria-label="主题颜色">
          <button v-for="preset in THEME_COLOR_PRESETS" :key="preset.value" type="button" class="swatch" :data-color="preset.value" :aria-label="preset.label" :title="preset.label" :aria-pressed="appStore.config.primaryColor.toLowerCase() === preset.value.toLowerCase()" @click="appStore.togglePrimaryColor(preset.value)">
            <span class="swatch-face" :style="{ backgroundColor: preset.value, color: checkColor(preset.value) }"><AppIcon v-if="appStore.config.primaryColor.toLowerCase() === preset.value.toLowerCase()" icon="lucide:check" /></span>
          </button>
          <span v-if="isCustomColor" class="custom-swatch" :title="`自定义颜色: ${appStore.config.primaryColor}`" :aria-label="`自定义颜色: ${appStore.config.primaryColor}`" role="img" :style="{ backgroundColor: appStore.config.primaryColor, color: checkColor(appStore.config.primaryColor) }"><AppIcon icon="lucide:check" /></span>
        </div>
      </div>
      <button ref="paletteTrigger" type="button" class="tool palette-trigger" aria-label="主题颜色" title="主题颜色" :aria-expanded="isPaletteOpen" @click="isPaletteOpen = !isPaletteOpen"><AppIcon icon="lucide:palette" /></button>
    </div>
    <button type="button" class="tool theme-trigger" :title="appStore.isDark ? '切换为浅色' : '切换为深色'" :aria-label="appStore.isDark ? '切换为浅色' : '切换为深色'" :aria-pressed="appStore.isDark" @click="appStore.toggleTheme(!appStore.isDark)"><AppIcon :icon="appStore.isDark ? 'lucide:sun' : 'lucide:moon'" /></button>
  </div>
</template>

<style scoped>
  .appearance-tools { display: flex; align-items: center; flex-shrink: 0; padding: 5px; border-radius: 24px; background: var(--login-toolbar); }
  .palette-host { display: flex; align-items: center; }
  .tool { display: grid; place-items: center; width: 32px; height: 32px; min-width: 0; padding: 0; border: 0; border-radius: 50%; color: var(--login-muted); background: transparent; cursor: pointer; }
  .tool :deep(.app-icon) { width: 17px; height: 17px; font-size: 17px; }
  .tool:hover, .tool[aria-expanded=true] { color: var(--el-color-primary); background: color-mix(in srgb, var(--el-color-primary) 10%, transparent); }
  .palette-trigger { color: var(--el-color-primary); }
  .tool:disabled { opacity: .6; cursor: wait; }
  .tool:focus-visible, .swatch:focus-visible { outline: 2px solid var(--el-color-primary); outline-offset: 2px; }
  .swatches-wrap { display: grid; grid-template-columns: 0fr; transition: grid-template-columns 250ms ease; }
  .is-open .swatches-wrap { grid-template-columns: 1fr; }
  .swatches { display: flex; align-items: center; min-width: 0; overflow: hidden; }
  .swatch { display: grid; place-items: center; flex-shrink: 0; width: 31px; height: 32px; padding: 0; border: 0; border-radius: 50%; background: transparent; cursor: pointer; }
  .swatch-face, .custom-swatch { display: grid; place-items: center; width: 20px; height: 20px; border-radius: 50%; }
  .custom-swatch { flex-shrink: 0; margin-inline: 6px; }
  .swatch-face :deep(.app-icon), .custom-swatch :deep(.app-icon) { width: 12px; height: 12px; font-size: 12px; }
  @media (max-width: 767px) {
    .appearance-tools { position: relative; padding: 4px; }
    .tool { width: 29px; height: 29px; }
    .swatches-wrap { display: none; position: absolute; top: calc(100% + 10px); right: 0; padding: 6px; border: 1px solid var(--login-border); border-radius: 20px; background: var(--login-canvas); box-shadow: 0 10px 30px #18202c1a; }
    .is-open .swatches-wrap { display: grid; }
  }
  @media (prefers-reduced-motion: reduce) { .swatches-wrap { transition: none; } }
</style>
