const maximumPlaintextBytes = 446

const fromBase64 = (value: string): ArrayBuffer => {
  const binary = atob(value)
  return Uint8Array.from(binary, (char) => char.charCodeAt(0))
    .buffer as ArrayBuffer
}

const toBase64 = (value: ArrayBuffer): string => {
  const bytes = new Uint8Array(value)
  let binary = ''
  for (const byte of bytes) binary += String.fromCharCode(byte)
  return btoa(binary)
}

export async function importPasswordPublicKey(pem: string): Promise<CryptoKey> {
  const encoded = pem
    .replace(/-----BEGIN PUBLIC KEY-----|-----END PUBLIC KEY-----/g, '')
    .replace(/\s/g, '')
  if (!encoded) throw new Error('密码传输公钥文件格式错误')

  const key = await crypto.subtle.importKey(
    'spki',
    fromBase64(encoded),
    { name: 'RSA-OAEP', hash: 'SHA-256' },
    false,
    ['encrypt']
  )
  if ((key.algorithm as RsaHashedKeyAlgorithm).modulusLength !== 4096) {
    throw new Error('密码传输必须使用 4096 位 RSA 公钥')
  }
  return key
}

export async function encryptWithPublicKey(
  txt: string,
  key: CryptoKey
): Promise<string> {
  const plainBytes = new TextEncoder().encode(txt)
  if (plainBytes.byteLength > maximumPlaintextBytes) {
    throw new Error(`密码不能超过 ${maximumPlaintextBytes} 个 UTF-8 字节`)
  }
  const encrypted = await crypto.subtle.encrypt(
    { name: 'RSA-OAEP' },
    key,
    plainBytes
  )
  return toBase64(encrypted)
}
