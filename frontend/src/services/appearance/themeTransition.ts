/**
 * 浏览器视图过渡 API 的能力声明，调用方需处理不支持的环境。
 */
interface AppearanceTransition {
  /** 当前状态是否已准备就绪。 */
  ready: Promise<void>
  /** 当前操作是否已经结束。 */
  finished: Promise<void>
  /**
   * 跳过当前浏览器视图过渡动画。
   */
  skipTransition(): void
}

/**
 * 浏览器视图过渡 API 的能力声明，调用方需处理不支持的环境。
 */
type AppearanceDocument = Document & {
  /**
   *  浏览器提供的视图过渡能力，不支持时不参与动画。
   * @returns 浏览器提供的视图过渡对象，可等待更新与动画完成。
   * @param update 在视图过渡截图之间执行的主题状态更新。
   */
  startViewTransition?: (update: () => Promise<void>) => AppearanceTransition
}

/**
 * transitionTheme。
 * @param update 参数。
 * @param dark 参数。
 * @param event 事件。
 */
export async function transitionTheme(
  update: () => Promise<void>,
  dark: boolean,
  event?: Event
): Promise<void> {
  const appearanceDocument = document as AppearanceDocument
  if (
    !event
    ||
    !appearanceDocument.startViewTransition
    ||
    window.matchMedia('(prefers-reduced-motion: reduce)').matches
    ||
    document.visibilityState !== 'visible'
  ) {
    await update()
    return
  }

  // 键盘触发没有鼠标坐标，使用触发控件中心，避免从屏幕左上角展开。
  const bounds =
    event.currentTarget instanceof Element
      ? event.currentTarget.getBoundingClientRect()
      : undefined
  let x: number
  if (event instanceof MouseEvent && event.detail > 0) {
    x = event.clientX
  } else {
    if (bounds) {
      x = bounds.left + bounds.width / 2
    } else {
      x = window.innerWidth / 2
    }
  }
  let y: number
  if (event instanceof MouseEvent && event.detail > 0) {
    y = event.clientY
  } else {
    if (bounds) {
      y = bounds.top + bounds.height / 2
    } else {
      y = window.innerHeight / 2
    }
  }
  const radius = Math.hypot(
    Math.max(x, window.innerWidth - x),
    Math.max(y, window.innerHeight - y)
  )
  const clips = [
    `circle(0px at ${x}px ${y}px)`,
    `circle(${radius}px at ${x}px ${y}px)`
  ]
  const root = document.documentElement
  let transition: AppearanceTransition | undefined
  let updated = false
  /**
   * 将已校验的配置或状态应用到当前界面。
   */
  const apply = async () => {
    await update()
    updated = true
  }

  root.classList.add('theme-transition')
  try {
    transition = appearanceDocument.startViewTransition(apply)
    await transition.ready
    const animation = root.animate(
      { clipPath: dark ? [...clips].reverse() : clips },
      {
        duration: 450,
        easing: 'ease-in',
        pseudoElement: dark
          ? '::view-transition-old(root)'
          : '::view-transition-new(root)'
      }
    )
    await animation.finished
  } catch {
    // 隐藏页面、取消快照或不支持伪元素动画时，模式仍需可靠地切换。
    if (!updated) await apply()
  } finally {
    transition?.skipTransition()
    await transition?.finished.catch(() => undefined)
    root.classList.remove('theme-transition')
  }
}
