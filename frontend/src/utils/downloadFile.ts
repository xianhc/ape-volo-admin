/**
 * 下载文件
 * @function downloadFile
 * @description 通过创建临时 a 标签下载文件
 *
 * @param {Blob | ArrayBuffer | string} obj - 文件内容（Blob、ArrayBuffer、字符串等）
 * @param {string} fileName - 下载的文件名
 *
 * @example
 * // 下载文本文件
 * downloadFile('Hello World', 'test.txt')
 *
 * // 下载 Blob 数据
 * const blob = new Blob([data], { type: 'application/pdf' })
 * downloadFile(blob, 'document.pdf')
 */
export const downloadFile = (
  obj: Blob | ArrayBuffer | string,
  fileName: string
): void => {
  const url = window.URL.createObjectURL(new Blob([obj]))
  const link = document.createElement('a')
  link.style.display = 'none'
  link.href = url
  link.setAttribute('download', fileName)
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
}

