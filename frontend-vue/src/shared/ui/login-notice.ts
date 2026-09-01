export const LOCKOUT_NOTICE = 'lockout'
export const ACCESS_DENIED_NOTICE = 'access-denied'
export const INVALID_USER_NOTICE = 'invalid-user'

export type LoginNoticeKey = typeof LOCKOUT_NOTICE | typeof ACCESS_DENIED_NOTICE | typeof INVALID_USER_NOTICE

export interface LoginNotice {
  title: string
  alert: string
  paragraphs: string[]
}

export const LOGIN_NOTICES: Record<LoginNoticeKey, LoginNotice> = {
  [LOCKOUT_NOTICE]: {
    title: 'Временная блокировка аккаунта',
    alert: 'Извините, но ваш аккаунт был временно заблокирован.',
    paragraphs: [
      'Это могло произойти из-за нескольких неудачных попыток входа. Пожалуйста, попробуйте снова через некоторое время.',
      'Если вы считаете, что это ошибка, свяжитесь с нашей службой поддержки.',
    ],
  },
  [ACCESS_DENIED_NOTICE]: {
    title: 'Извините, доступ запрещен',
    alert: 'Извините, но у вас нет разрешения на доступ к этому ресурсу. Если вы считаете, что это ошибка, пожалуйста, свяжитесь с администратором.',
    paragraphs: [],
  },
  [INVALID_USER_NOTICE]: {
    title: 'Ошибка входа',
    alert: 'Извините, но мы не можем найти вашу учетную запись. Пожалуйста, проверьте введенные данные и попробуйте снова.',
    paragraphs: [],
  },
}
