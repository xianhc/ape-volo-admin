import './style/ape-volo-admin.scss'
import 'element-plus/theme-chalk/dark/css-vars.css'
import 'element-plus/dist/index.css'
import 'uno.css'
import { createApp } from 'vue'
import setupPlugins from '@/plugins'
import App from './App.vue'

const app = createApp(App)
app.use(setupPlugins)
app.mount('#app')
export default app
