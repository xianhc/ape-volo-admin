<template>
  <div class="ape-volo-bg p-4 lg:p-6 h-[calc(100vh-180px)] overflow-y-scroll">
    <div class="grid lg:grid-cols-12 md:grid-cols-1 gap-8">
      <div class="lg:col-span-3">
        <div class="ape-volo-content rounded-xl p-6 mb-6 profile-card">
          <h2 class="text-lg font-semibold mb-4 flex items-center gap-2">
            <el-icon class="text-blue-500"><AppIcon icon="ep:info-filled" /></el-icon>
            基础信息
          </h2>
          <div class="space-y-4">
            <div class="flex justify-center mb-4">
              <el-upload
                ref="uploadRef"
                :action="`${getBaseUrl()}/user/update-avatar`"
                name="file"
                accept=".jpg,.jpeg,.png,.gif,.bmp,.webp"
                :show-file-list="false"
                :auto-upload="false"
                :on-success="handleImageSuccess"
                :on-change="handleFileChange"
                :headers="headers"
              >
                <img
                  style="
                    width: 120px;
                    height: 120px;
                    border-radius: 50%;
                    object-fit: cover;
                  "
                  :src="getStaticBaseUrl() + userStore.userInfo.headerImg"
                  title="点击上传头像"
                  class="avatar"
                  alt="用户头像"
                />
              </el-upload>
            </div>
            <div
              class="flex items-center justify-between gap-1 lg:gap-3 text-gray-600 dark:text-gray-300"
            >
              <div class="flex items-center gap-1 lg:gap-3">
                <el-icon><AppIcon icon="ep:user" /></el-icon>
                <span class="font-medium">登录账号</span>
              </div>
              <span>{{ userStore.userInfo.userName }}</span>
            </div>
            <div
              class="flex items-center justify-between gap-1 lg:gap-3 text-gray-600 dark:text-gray-300"
            >
              <div class="flex items-center gap-1 lg:gap-3">
                <el-icon><AppIcon icon="svg:nick-name" /></el-icon>
                <span class="font-medium">用户昵称</span>
              </div>
              <span>{{ userStore.userInfo.nickName }}</span>
            </div>
            <div
              class="flex items-center justify-between gap-1 lg:gap-3 text-gray-600 dark:text-gray-300"
            >
              <div class="flex items-center gap-1 lg:gap-3">
                <el-icon><AppIcon icon="svg:dept" /></el-icon>
                <span class="font-medium">所属部门</span>
              </div>
              <span>{{ userStore.userInfo.dept.name }}</span>
            </div>
            <div
              class="flex items-center justify-between gap-1 lg:gap-3 text-gray-600 dark:text-gray-300"
            >
              <div class="flex items-center gap-1 lg:gap-3">
                <el-icon><AppIcon icon="ep:phone" /></el-icon>
                <span class="font-medium">手机号码</span>
              </div>
              <span>{{ userStore.userInfo.phone }}</span>
            </div>
            <div
              class="flex items-center justify-between gap-1 lg:gap-3 text-gray-600 dark:text-gray-300"
            >
              <div class="flex items-center gap-1 lg:gap-3">
                <el-icon><AppIcon icon="ep:message" /></el-icon>
                <span class="font-medium">用户邮箱</span>
              </div>
              <span>{{ userStore.userInfo.email }}</span>
            </div>
            <div
              class="flex items-center justify-between gap-1 lg:gap-3 text-gray-600 dark:text-gray-300"
            >
              <div class="flex items-center gap-1 lg:gap-3">
                <el-icon><AppIcon icon="ep:lock" /></el-icon>
                <span class="font-medium">安全设置</span>
              </div>
              <div class="flex gap-1 ml-auto">
                <el-button
                  link
                  type="primary"
                  @click="showUpdateEmailPanel = true"
                >
                  修改邮箱
                </el-button>
                <el-button
                  link
                  type="primary"
                  @click="showUpdatePassPanel = true"
                >
                  修改密码
                </el-button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 右侧内容区 -->
      <div class="lg:col-span-9">
        <div class="ape-volo-content rounded-xl p-6 profile-card">
          <el-tabs
            v-model="activeTab"
            class="custom-tabs"
            @tab-click="onTabClick"
          >
            <el-tab-pane name="profile">
              <template #label>
                <div class="flex items-center gap-2">
                  <el-icon><AppIcon icon="ep:postcard" /></el-icon>
                  用户资料
                </div>
              </template>
              <div class="grid grid-cols-2 md:grid-cols-4 gap-4 lg:gap-6 py-6">
                <el-form
                  ref="form"
                  :model="userForm"
                  :rules="rulesForm"
                  label-width="65px"
                >
                  <el-form-item label="昵称" prop="nickName">
                    <el-input v-model="userForm.nickName" />
                  </el-form-item>
                  <el-form-item label="手机号" prop="phone">
                    <el-input v-model="userForm.phone" />
                  </el-form-item>
                  <el-form-item label="性别">
                    <el-radio-group v-model="userForm.genderCode">
                      <el-radio
                        v-for="item in genderOption"
                        :key="item.id"
                        border
                        :value="Number(item.value)"
                      >
                        {{ item.label }}
                      </el-radio>
                    </el-radio-group>
                  </el-form-item>
                  <el-form-item label="">
                    <el-button
                      type="primary"
                      :loading="loading"
                      :disabled="!formChanged"
                      @click="handleSave"
                    >
                      保存配置
                    </el-button>
                  </el-form-item>
                </el-form>
              </div>
            </el-tab-pane>
            <el-tab-pane name="record">
              <template #label>
                <div class="flex items-center gap-2">
                  <el-icon><AppIcon icon="ep:operation" /></el-icon>
                  行为记录
                </div>
              </template>
              <OperateLogSection ref="logSectionRef" />
            </el-tab-pane>
          </el-tabs>
        </div>
      </div>
    </div>
    <!-- 修改密码对话框 -->
    <updatePassPanel v-model:show-update-pass-panel="showUpdatePassPanel" />

    <!-- 修改邮箱对话框 -->
    <updateEmailPanel v-model:show-update-email-panel="showUpdateEmailPanel" />

    <!-- 修改头像 -->
    <el-dialog
      v-model="dialogVisible"
      title="图片裁剪"
      width="1200px"
      append-to-body
      :close-on-click-modal="false"
      draggable
      @close="dialogVisible = false"
    >
      <div class="flex gap-[30px] h-[600px]">
        <!-- 左侧编辑区 -->
        <div class="flex flex-col flex-1">
          <div class="flex-1 bg-[#f8f8f8] rounded-lg overflow-hidden">
            <VueCropper
              ref="cropperRef"
              :img="imgSrc"
              output-type="jpeg"
              :auto-crop="true"
              :auto-crop-width="cropWidth"
              :auto-crop-height="cropHeight"
              :fixed-box="false"
              :fixed="fixedRatio"
              :fixed-number="fixedNumber"
              :center-box="true"
              :can-move-box="true"
              :full="false"
              :max-img-size="1200"
              :original="true"
              @real-time="handleRealTime"
            ></VueCropper>
          </div>

          <!-- 工具栏 -->
          <div
            class="mt-[20px] flex items-center p-[10px] bg-white rounded-lg shadow-[0_2px_12px_rgba(0,0,0,0.1)]"
          >
            <el-button-group>
              <el-tooltip content="向左旋转">
                <el-button :icon="createIconifyIcon('ep:refresh-left')" @click="rotate(-90)" />
              </el-tooltip>
              <el-tooltip content="向右旋转">
                <el-button :icon="createIconifyIcon('ep:refresh-right')" @click="rotate(90)" />
              </el-tooltip>
              <el-button :icon="createIconifyIcon('ep:plus')" @click="changeScale(1)"></el-button>
              <el-button :icon="createIconifyIcon('ep:minus')" @click="changeScale(-1)"></el-button>
            </el-button-group>

            <el-select
              v-model="currentRatio"
              placeholder="选择比例"
              class="w-32 ml-4"
              @change="onCurrentRatio"
            >
              <el-option
                v-for="(item, index) in ratioOptions"
                :key="index"
                :label="item.label"
                :value="index"
              />
            </el-select>
          </div>
        </div>

        <!-- 右侧预览区 -->
        <div class="w-[340px]">
          <div
            class="bg-white p-5 rounded-lg shadow-[0_2px_12px_rgba(0,0,0,0.1)]"
          >
            <div class="mb-[15px] text-gray-600">裁剪预览</div>
            <div
              class="bg-white p-5 rounded-lg shadow-[0_2px_12px_rgba(0,0,0,0.1)]"
              :style="{ width: previews.w + 'px', height: previews.h + 'px' }"
            >
              <div class="w-full h-full relative overflow-hidden">
                <img
                  :src="previews.url"
                  :style="previews.img"
                  alt=""
                  class="max-w-none absolute transition-all duration-300 ease-in-out image-render-pixelated origin-[0_0]"
                />
              </div>
            </div>
          </div>
        </div>
      </div>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="dialogVisible = false">取 消</el-button>
          <el-button type="primary" :loading="uploading" @click="handleUpload">
            {{ uploading ? '上传中...' : '上 传' }}
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
  import { reactive, ref, computed, getCurrentInstance } from 'vue'
  import { cloneDeep, isEqual } from 'lodash-es'
  import { ElMessage } from 'element-plus'
  import { useUserStore } from '@/pinia/modules/user'
  import { isvalidPhone } from '@/utils/validate'
  import { getDict, type DictOption } from '@/composables/useDictionary'
  import { editUserCenter } from '@/api/identity/user'
  import updatePassPanel from './module/updatePassPanel.vue'
  import updateEmailPanel from './module/updateEmailPanel.vue'
  import OperateLogSection from './module/OperateLogSection.vue'
  import { getBaseUrl, getStaticBaseUrl } from '@/utils/index'
  import { getApeToken } from '@/services/session/tokenStorage'
  import { createIconifyIcon } from '@/components/icons'
  import 'vue-cropper/dist/index.css'
  import { VueCropper } from 'vue-cropper'
  import type {
    TabsPaneContext,
    UploadFile,
    UploadInstance,
    FormInstance
  } from 'element-plus'

  // 定义 VueCropper 实例类型
  // interface CropperInstance {
  //   rotateLeft: () => void
  //   rotateRight: () => void
  //   changeScale: (scale: number) => void
  //   getCropBlob: (callback: (blob: Blob) => void) => void
  // }

  // 定义预览数据类型
  interface PreviewData {
    w?: number
    h?: number
    url?: string
    img?: string
    [key: string]: any
  }

  defineOptions({
    name: 'PersonalCenter'
  })
  const userStore = useUserStore()
  // 声明并初始化 form 对象
  const userForm = reactive({
    nickName: userStore.userInfo.nickName,
    phone: userStore.userInfo.phone,
    genderCode: userStore.userInfo.genderCode
  })

  var apeToken = getApeToken()
  const headers = ref({
    Authorization: apeToken.tokenType + ' ' + apeToken.accessToken
  })

  const uploadRef = ref<UploadInstance | null>(null)
  // 响应式数据
  const dialogVisible = ref(false)
  const imgSrc = ref('')
  // const cropperRef = ref<CropperInstance | null>(null)
  const instance = getCurrentInstance()
  const proxy = instance?.proxy as any
  const previews = ref<PreviewData>({})
  const uploading = ref(false)

  const activeTab = ref('profile')
  const showUpdatePassPanel = ref(false)
  const showUpdateEmailPanel = ref(false)
  const loading = ref(false)

  /** 行为记录面板实例，切换到该标签页时触发加载。 */
  const logSectionRef = ref<InstanceType<typeof OperateLogSection> | null>(null)

  //字典
  const genderOption = ref<DictOption[]>([])

  const validPhone = (_rule: any, value: any, callback: any) => {
    if (value && !isvalidPhone(value)) {
      callback(new Error('请输入正确的11位手机号码'))
    } else {
      callback()
    }
  }
  const rulesForm = {
    nickName: [{ required: true, message: '请输入昵称', trigger: 'blur' }],
    phone: [{ trigger: 'blur', validator: validPhone }]
  }

  getDict('gender_code')
    .then((res) => {
      genderOption.value = res
    })
    .catch((err) => {
      console.error('获取 gender_code 失败:', err)
    })

  const originalForm = reactive(cloneDeep(userForm))
  const formChanged = computed(() => !isEqual(userForm, originalForm))
  const form = ref<FormInstance | null>(null)
  const handleSave = async () => {
    loading.value = true
    form.value?.validate(async (valid: boolean) => {
      if (valid) {
        try {
          await editUserCenter({
            nickName: userForm.nickName,
            genderCode: userForm.genderCode,
            phone: userForm.phone
          })
          userStore.ResetUserInfo({
            nickName: userForm.nickName,
            phone: userForm.phone,
            genderCode: userForm.genderCode
          })
          Object.assign(originalForm, cloneDeep(userForm))
          ElMessage.success('修改成功！')
        } catch {
          /* empty */
        } finally {
          loading.value = false
        }
      } else {
        loading.value = false
      }
    })
  }

  /** 切换到行为记录标签页时按需加载数据。 */
  const onTabClick = (tab: TabsPaneContext): void => {
    if (tab.paneName === 'record') {
      void logSectionRef.value?.load()
    }
  }

  // 缩放控制
  const changeScale = (value: number) => {
    proxy.$refs.cropperRef.changeScale(value)
  }

  // 比例预设
  const ratioOptions = ref([
    { label: '1:1', value: [1, 1] },
    { label: '16:9', value: [16, 9] },
    { label: '9:16', value: [9, 16] },
    { label: '4:3', value: [4, 3] },
    { label: '自由比例', value: [] }
  ])

  const fixedNumber = ref([1, 1])
  const cropWidth = ref(300)
  const cropHeight = ref(300)

  const fixedRatio = ref(false)
  const currentRatio = ref(4)
  const onCurrentRatio = () => {
    fixedNumber.value = ratioOptions.value[currentRatio.value].value
    switch (currentRatio.value) {
      case 0:
        cropWidth.value = 300
        cropHeight.value = 300
        fixedRatio.value = true
        break
      case 1:
        cropWidth.value = 300
        cropHeight.value = (300 * 9) / 16
        fixedRatio.value = true
        break
      case 2:
        cropWidth.value = (300 * 9) / 16
        cropHeight.value = 300
        fixedRatio.value = true
        break
      case 3:
        cropWidth.value = 300
        cropHeight.value = (300 * 3) / 4
        fixedRatio.value = true
        break
      default:
        cropWidth.value = 300
        cropHeight.value = 300
        fixedRatio.value = false
    }
  }

  // 文件处理
  const handleFileChange = (file: UploadFile) => {
    if (!file.raw) return
    const isImage = file.raw.type.includes('image')
    if (!isImage) {
      ElMessage.error('请选择图片文件')
      return
    }

    if (file.raw.size / 1024 / 1024 > 8) {
      ElMessage.error('文件大小不能超过8MB!')
      return false
    }

    const reader = new FileReader()
    reader.onload = (e: ProgressEvent<FileReader>) => {
      if (e.target && typeof e.target.result === 'string') {
        imgSrc.value = e.target.result
        dialogVisible.value = true
      }
    }
    reader.readAsDataURL(file.raw)
  }

  // 旋转控制
  const rotate = (degree: number) => {
    if (degree === -90) {
      proxy.$refs.cropperRef.rotateLeft()
    } else {
      proxy.$refs.cropperRef.rotateRight()
    }
  }

  // 实时预览
  const handleRealTime = (data: any) => {
    previews.value = data
  }

  // 上传处理
  const handleUpload = () => {
    uploading.value = true
    proxy.$refs.cropperRef.getCropBlob((blob: Blob) => {
      try {
        const file = new File([blob], `${Date.now()}.jpg`, {
          type: 'image/jpeg'
        })
        uploadRef.value?.clearFiles()
        uploadRef.value?.handleStart(file as any)
        uploadRef.value?.submit()
      } catch (error) {
        uploading.value = false
        const errorMessage = error instanceof Error ? error.message : '未知错误'
        ElMessage.error('上传失败: ' + errorMessage)
      }
    })
  }

  const handleImageSuccess = (res: any) => {
    const closeDialog = () => {
      uploading.value = false
      dialogVisible.value = false
    }
    try {
      if (res.status === 201) {
        setTimeout(() => {
          closeDialog()
          previews.value = {}
          ElMessage.success(res.message || '头像上传成功')
        }, 1000)
      } else {
        closeDialog()
        ElMessage.error(res.message || '头像上传失败')
      }
    } catch (error) {
      closeDialog()
      const errorMessage = error instanceof Error ? error.message : '未知错误'
      ElMessage.error('头像上传处理异常: ' + errorMessage)
    }
  }
</script>

<style scoped>
  :deep(.vue-cropper) {
    background: transparent;
  }
</style>
