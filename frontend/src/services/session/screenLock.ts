/**
 * 锁屏状态读写。
 */
import {
  readStoredJson,
  writeStoredJson,
  removeStoredValue
} from '@/services/storage/appStorage'

const SCREEN_LOCK_STORAGE_KEY = 'screen-lock'

/**
 * 本地屏幕锁状态，保存会话归属和派生校验信息。
 */
export interface ScreenLockRecord {
  /** 客户端保存的会话标识，用于隔离不同登录。 */
  clientSessionId: string
  /** 本地屏幕锁校验值派生使用的随机盐。 */
  salt: string
  /** 由屏幕锁密码派生的本地校验值，不保存明文密码。 */
  verifier: string
}

/**
 * 校验本地屏幕锁记录的必要字段。
 * @param value 本次转换、校验或更新所接收的值。
 * @returns 输入满足目标类型要求的必要字段与值约束时返回 true。
 */
const isScreenLockRecord = (value: unknown): value is ScreenLockRecord =>
  typeof value === 'object' &&
  value !== null &&
  'clientSessionId' in value &&
  typeof value.clientSessionId === 'string' &&
  value.clientSessionId.length > 0 &&
  'salt' in value &&
  typeof value.salt === 'string' &&
  /^[a-f0-9]{32}$/.test(value.salt) &&
  'verifier' in value &&
  typeof value.verifier === 'string' &&
  /^[a-f0-9]{64}$/.test(value.verifier)

/**
 * 读取并校验当前标签页保存的屏幕锁记录。
 * @returns 有效的屏幕锁记录；未设置或记录无效时为 null。
 */
export const readScreenLockRecord = (): ScreenLockRecord | null =>
  readStoredJson<ScreenLockRecord | null>(
    sessionStorage,
    SCREEN_LOCK_STORAGE_KEY,
    null,
    (value): value is ScreenLockRecord | null =>
      value === null || isScreenLockRecord(value)
  )

/**
 * 写入屏幕锁记录并立即回读，确认浏览器实际保存了同一会话和派生校验信息。
 * @param record 包含会话归属、随机盐及密码派生校验值的记录。
 * @returns 回读字段与提交记录一致时返回 true，否则返回 false。
 */
export function saveScreenLockRecord(record: ScreenLockRecord): boolean {
  writeStoredJson(sessionStorage, SCREEN_LOCK_STORAGE_KEY, record)
  const saved = readScreenLockRecord()
  return (
    saved?.clientSessionId === record.clientSessionId &&
    saved.salt === record.salt &&
    saved.verifier === record.verifier
  )
}

/**
 * 移除当前标签页的屏幕锁记录。
 */
export const clearScreenLockRecord = (): void =>
  removeStoredValue(sessionStorage, SCREEN_LOCK_STORAGE_KEY)

/**
 * 将派生校验字节编码为补齐两位的小写十六进制文本。
 * @param bytes 待编码或格式化的字节数据。
 * @returns 字节数据的十六进制文本。
 */
const hex = (bytes: Uint8Array): string =>
  Array.from(bytes, (byte) => byte.toString(16).padStart(2, '0')).join('')

/**
 * 使用随机盐从屏幕锁密码派生本地校验值。
 * @param password 密码策略配置或身份认证输入中的密码，不能记录到日志。
 * @param salt 本地屏幕锁校验值派生使用的随机盐。
 * @returns 可用于本地屏幕锁比较的派生校验文本。
 */
async function deriveVerifier(password: string, salt: string): Promise<string> {
  const key = await crypto.subtle.importKey(
    'raw',
    new TextEncoder().encode(password),
    'PBKDF2',
    false,
    ['deriveBits']
  )
  const bytes = new Uint8Array(
    Array.from({ length: salt.length / 2 }, (_, index) =>
      Number.parseInt(salt.slice(index * 2, index * 2 + 2), 16)
    )
  )
  const result = await crypto.subtle.deriveBits(
    { name: 'PBKDF2', salt: bytes, iterations: 150_000, hash: 'SHA-256' },
    key,
    256
  )
  return hex(new Uint8Array(result))
}

/**
 * 生成随机盐并通过 PBKDF2 派生密码校验值，创建属于指定登录会话的屏幕锁记录。
 * @param clientSessionId 需要锁定的客户端登录会话标识。
 * @param password 用于本地屏幕锁验证的密码。
 * @returns 可存入当前标签页的屏幕锁记录，不含明文密码。
 */
export async function createScreenLockRecord(
  clientSessionId: string,
  password: string
): Promise<ScreenLockRecord> {
  const salt = hex(crypto.getRandomValues(new Uint8Array(16)))
  return {
    clientSessionId,
    salt,
    verifier: await deriveVerifier(password, salt)
  }
}

/**
 * 用记录中的盐派生输入密码的校验值，遍历整个校验文本比较差异。
 * @param record 已经通过结构校验的屏幕锁记录。
 * @param password 用户本次输入的解锁密码。
 * @returns 派生结果与记录中的校验值一致时返回 true。
 */
export async function verifyScreenLockPassword(
  record: ScreenLockRecord,
  password: string
): Promise<boolean> {
  const actual = await deriveVerifier(password, record.salt)
  let difference = 0
  for (let index = 0;index < actual.length;index++)
    difference |= actual.charCodeAt(index) ^ record.verifier.charCodeAt(index)
  return difference === 0
}
