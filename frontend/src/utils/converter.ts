/**
 * 将字符串转换为布尔值
 * @function strToBool
 * @description 将字符串形式的布尔值转换为真正的布尔类型
 *
 * 转换规则：
 * - 字符串 'true'（忽略大小写）→ true
 * - 其他所有值 → false
 * - null 或 undefined → false
 *
 * @param {string | null | undefined} inputStr - 输入的字符串
 * @returns {boolean} 转换后的布尔值
 *
 * @example
 * strToBool('true') // true
 * strToBool('True') // true
 * strToBool('TRUE') // true
 * strToBool('false') // false
 * strToBool('any other string') // false
 * strToBool(null) // false
 * strToBool(undefined) // false
 */
export const strToBool = (inputStr: string | null | undefined): boolean => {
  if (inputStr !== null && inputStr !== undefined) {
    return String(inputStr).toLowerCase() === 'true'
  } else {
    return false
  }
}

