import type { IconifyIcon } from '@iconify/vue'

/**
 * 将仓库内 SVG 模块转换为 Iconify 图标数据。
 *
 * 不接受用户上传或接口返回的 SVG 文本。
 * @param source 仓库内 SVG 模块的原始文本。
 * @returns 保留视图边界和 SVG 图形内容的 Iconify 数据。
 */
export function toIconData(source: string): IconifyIcon {
  const svg = new DOMParser().parseFromString(
    source,
    'image/svg+xml'
  ).documentElement
  if (svg.localName !== 'svg' || svg.querySelector('parsererror'))
    throw new Error('Invalid local SVG')
  const viewBox = svg
    .getAttribute('viewBox')
    ?.trim()
    .split(/[\s,]+/)
    .map(Number)
  const width =
    viewBox?.[2] ?? (parseFloat(svg.getAttribute('width') ?? '') || 24)
  const height =
    viewBox?.[3] ?? (parseFloat(svg.getAttribute('height') ?? '') || 24)
  const presentation = document.createElementNS(
    'http://www.w3.org/2000/svg',
    'g'
  )
  for (const { name, value } of [...svg.attributes]) {
    if (
      [
        'fill',
        'stroke',
        'stroke-width',
        'stroke-linecap',
        'stroke-linejoin',
        'fill-rule',
        'clip-rule',
        'opacity',
        'style'
      ].includes(name)
    )
      presentation.setAttribute(name, value)
  }
  presentation.innerHTML = svg.innerHTML
  return {
    body: presentation.outerHTML,
    width,
    height,
    left: viewBox?.[0] ?? 0,
    top: viewBox?.[1] ?? 0
  }
}
