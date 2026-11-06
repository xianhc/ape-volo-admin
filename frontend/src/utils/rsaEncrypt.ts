import { encryptWithPublicKey, importPasswordPublicKey } from './rsaCrypto'

/** 每次密码操作读取部署目录中的公钥，替换文件后无需重新构建或刷新页面。 */
export async function loadPasswordPublicKey(
  baseUrl: string = import.meta.env.BASE_URL
): Promise<CryptoKey> {
  const response = await fetch(`${baseUrl}keys/password-public.pem`, {
    cache: 'no-store'
  })
  if (!response.ok) {
    throw new Error(`密码传输公钥加载失败：HTTP ${response.status}`)
  }
  return importPasswordPublicKey(await response.text())
}

/** 同一次操作的多个密码字段应传入同一公钥；未传入时重新加载。 */
export async function encrypt(txt: string, publicKey?: CryptoKey): Promise<string> {
  return encryptWithPublicKey(txt, publicKey ?? (await loadPasswordPublicKey()))
}
