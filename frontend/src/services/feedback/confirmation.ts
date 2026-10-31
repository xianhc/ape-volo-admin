/**
 * 操作确认弹窗（本项目无多语言，文案由调用方传入中文）。
 */
import { ElMessageBox } from 'element-plus'

/**
 * 弹出确认框，用户确认返回 true，取消或关闭返回 false。
 * @param message 确认提示正文。
 * @param title 弹窗标题。
 * @returns 用户是否确认。
 */
export async function confirmAction(
  message: string,
  title = '操作确认'
): Promise<boolean> {
  try {
    await ElMessageBox.confirm(message, title, {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    return true
  } catch {
    return false
  }
}
