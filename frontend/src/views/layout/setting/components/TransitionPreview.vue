<template>
  <div
    class="transition-grid"
    role="radiogroup"
    :aria-label="'页面切换动画'"
  >
    <button
      v-for="option in options"
      :key="option.value"
      type="button"
      class="transition-option preference-choice"
      :class="{
        'is-selected': modelValue === option.value,
        'is-paused': !active
      }"
      role="radio"
      :aria-checked="modelValue === option.value"
      @click="emit('update:modelValue', option.value)"
    >
      <span class="transition-stage preference-preview-frame">
        <span class="transition-stage-content">
          <span class="transition-page" :class="`preview-${option.value}`">
            <span></span><span></span><span></span>
          </span>
        </span>
      </span>
      <span class="transition-label">{{ option.label }}</span>
      <AppIcon
        v-if="modelValue === option.value"
        icon="lucide:check"
        class="transition-check"
      />
    </button>
  </div>
</template>

<script setup lang="ts">
  /**
   * 页面切换动画预览组件。
   */
  import AppIcon from '@/components/AppIcon.vue'
  import { TransitionType } from '@/enums/TransitionType'
  import type { TransitionTypeValue } from '@/enums/TransitionType'
  defineOptions({ name: 'TransitionPreview' })
  defineProps<{
    /** 由父组件持有的受控绑定值。 */
    modelValue: TransitionTypeValue
    /** 日志队列中当前活动的消息数量。 */
    active: boolean
  }>()
  const emit = defineEmits<{
    /** 将新的绑定值通知给父组件。 */
    'update:modelValue': [value: TransitionTypeValue]
  }>()
  const options = [
    {
      value: TransitionType.Slide,
      /**
       * 面向用户显示的标签。
       * @returns 当前语言下对应条目的显示文案。
       */
      get label() {
        return '滑动'
      }
    },
    {
      value: TransitionType.Fade,
      /**
       * 面向用户显示的标签。
       * @returns 当前语言下对应条目的显示文案。
       */
      get label() {
        return '淡入淡出'
      }
    },
    {
      value: TransitionType.Zoom,
      /**
       * 面向用户显示的标签。
       * @returns 当前语言下对应条目的显示文案。
       */
      get label() {
        return '缩放'
      }
    },
    {
      value: TransitionType.None,
      /**
       * 面向用户显示的标签。
       * @returns 当前语言下对应条目的显示文案。
       */
      get label() {
        return '无动画'
      }
    }
  ]
</script>

<style scoped>
  .transition-grid {
    display: grid;
    grid-template-columns: repeat(4, minmax(0, 1fr));
    gap: 14px;
  }
  .transition-option {
    position: relative;
    display: grid;
    gap: 7px;
    min-width: 0;
    padding: 0;
    border: 0;
    border-radius: var(--ape-volo-radius);
    background: var(--app-surface);
    color: var(--app-text-secondary);
    font: inherit;
    cursor: pointer;
  }
  .transition-stage {
    display: grid;
    place-items: center;
    height: 84px;
    border: 1px solid var(--app-border);
    overflow: hidden;
    border-radius: var(--preference-radius, 10px);
    background: var(--app-surface);
  }
  .transition-stage-content {
    display: grid;
    place-items: center;
    width: 100%;
    height: 100%;
    overflow: hidden;
    border-radius: inherit;
  }
  .transition-page {
    display: grid;
    grid-template-rows: 8px repeat(2, 1fr);
    gap: 4px;
    width: 46px;
    height: 46px;
    padding: 6px;
    border: 1px solid var(--app-border);
    border-radius: 3px;
    background: color-mix(in srgb, var(--el-color-primary) 12%, var(--app-surface));
    box-shadow: var(--app-shadow-sm);
    animation-duration: 3s;
    animation-delay: -1.05s;
    animation-iteration-count: infinite;
    animation-timing-function: ease-in-out;
  }
  .transition-page span {
    border-radius: 2px;
    background: var(--app-border-strong);
  }
  .transition-page span:first-child {
    width: 55%;
    background: var(--el-color-primary);
  }
  .preview-slide {
    animation-name: preview-slide;
  }
  .preview-fade {
    animation-name: preview-fade;
  }
  .preview-zoom {
    animation-name: preview-zoom;
  }
  .preview-none {
    animation: none;
  }
  .is-paused .transition-page {
    animation-play-state: paused;
  }
  .transition-label {
    font-size: 12px;
    font-weight: 400;
  }
  .transition-check {
    position: absolute;
    top: 6px;
    right: 6px;
    width: 16px;
    height: 16px;
    border-radius: 50%;
    background: var(--el-color-primary);
    color: var(--app-primary-on);
  }
  @keyframes preview-slide {
    0%,
    20%,
    100% {
      opacity: 0;
      transform: translateX(-18px);
    }
    35%,
    80% {
      opacity: 1;
      transform: translateX(0);
    }
  }
  @keyframes preview-fade {
    0%,
    20%,
    100% {
      opacity: 0;
    }
    35%,
    80% {
      opacity: 1;
    }
  }
  @keyframes preview-zoom {
    0%,
    20%,
    100% {
      opacity: 0;
      transform: scale(0.78);
    }
    35%,
    80% {
      opacity: 1;
      transform: scale(1);
    }
  }
  @media (prefers-reduced-motion: reduce) {
    .transition-page {
      animation: none;
    }
  }
</style>
