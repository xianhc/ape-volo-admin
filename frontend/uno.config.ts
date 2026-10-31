import { defineConfig } from '@unocss/vite'
import presetIcons from '@unocss/preset-icons'
import presetWind3 from '@unocss/preset-wind3'
import transformerDirectives from '@unocss/transformer-directives'

export default defineConfig({
  theme: {
    backgroundColor: {
      main: '#F5F5F5'
    },
    textColor: {
      active: 'var(--el-color-primary)'
    },
    shadowColor: {
      active: 'var(--el-color-primary)'
    },
    borderColor: {
      'table-border': 'var(--el-border-color-lighter)',
      'input-hover': 'var(--el-color-primary)'
    }
  },
  presets: [presetWind3({ dark: 'class' }), presetIcons()],
  transformers: [transformerDirectives()]
})
