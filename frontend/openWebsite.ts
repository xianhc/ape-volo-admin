import { exec } from 'node:child_process'

const url = 'https://www.apevolo.com'

switch (process.platform) {
  case 'win32':
    exec(`start ${url}`)
    break
  case 'darwin':
    exec(`open ${url}`)
    break
}
