/**
 * 验证邮箱地址格式
 * @function validEmail
 * @description 使用正则表达式验证邮箱地址的标准格式
 *
 * 验证规则：
 * - 用户名部分：支持字母、数字、特殊字符
 * - @ 符号分隔
 * - 域名部分：支持多级域名和各种顶级域名
 * - 特殊字符处理：引号包裹的用户名
 *
 * @param {string} email - 要验证的邮箱地址
 * @returns {boolean} 如果邮箱格式正确返回 true，否则返回 false
 *
 * @example
 * validEmail('user@example.com') // true
 * validEmail('user.name@example.co.uk') // true
 * validEmail('user+tag@example.org') // true
 * validEmail('invalid.email') // false
 */
export function validEmail(email: string): boolean {
  const reg =
    // eslint-disable-next-line no-useless-escape
    /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/
  return reg.test(email)
}

/**
 * 验证中国大陆手机号码格式
 * @function isvalidPhone
 * @description 验证11位中国大陆手机号码格式
 *
 * 支持的号段：
 * - 13x 系列
 * - 14x 系列
 * - 15x 系列
 * - 17x 系列
 * - 18x 系列
 *
 * @param {string} phone - 要验证的手机号码
 * @returns {boolean} 如果手机号格式正确返回 true，否则返回 false
 *
 * @example
 * isvalidPhone('13812345678') // true
 * isvalidPhone('15987654321') // true
 * isvalidPhone('12345678901') // false
 */
export function isvalidPhone(phone: string): boolean {
  const reg = /^1[3|4|5|7|8][0-9]\d{8}$/
  return reg.test(phone)
}

