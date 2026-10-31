import passwordPublicKeyPem from '@/assets/.secrets/password-public.pem?raw'
import { encryptWithPublicKey, importPasswordPublicKey } from './rsaCrypto'

const publicKeyPromise = importPasswordPublicKey(passwordPublicKeyPem)

/** 使用前端项目内置的 PEM 公钥加密密码传输字段，不请求后端公钥接口。 */
export async function encrypt(txt: string): Promise<string> {
  return encryptWithPublicKey(txt, await publicKeyPromise)
}
