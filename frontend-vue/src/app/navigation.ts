import {
  mdAssessment,
  mdAttachMoney,
  mdCategory,
  mdElectricCar,
  mdFlashOn,
  mdList,
  mdMoney,
  mdPassword,
  mdPerson,
  mdPieChart,
  mdRepeat,
  mdSettingsApplications,
} from '@/shared/icons'

export interface NavItem {
  title: string
  to: string
  icon: string
  testId: string
  match?: 'prefix' | 'all'
}

export interface NavGroup {
  title: string
  icon: string
  items: NavItem[]
}

export type NavEntry = NavGroup | NavItem

export interface NavAccess {
  isAuthenticated: boolean
  userName: string | null
}

const authorizedEntries: NavEntry[] = [
  {
    title: 'Операции',
    icon: mdMoney,
    items: [
      { title: 'Список', to: '/operations', icon: mdList, testId: 'nav-operations' },
      { title: 'Статистика', to: '/statistics', icon: mdPieChart, testId: 'nav-statistics' },
      { title: 'Категории', to: '/categories', icon: mdCategory, testId: 'nav-categories' },
      { title: 'Быстрые', to: '/fast-operations', icon: mdFlashOn, testId: 'nav-fast-operations' },
      { title: 'Регулярные', to: '/regular-operations', icon: mdRepeat, testId: 'nav-regular-operations' },
    ],
  },
  {
    title: 'Долги',
    icon: mdAttachMoney,
    items: [
      { title: 'Список', to: '/debts', icon: mdList, testId: 'nav-debts', match: 'all' },
      { title: 'Отчёты', to: '/debts/accounts', icon: mdAssessment, testId: 'nav-debt-accounts' },
    ],
  },
  { title: 'Авто', to: '/cars', icon: mdElectricCar, testId: 'nav-cars' },
  { title: 'Настройки', to: '/settings', icon: mdSettingsApplications, testId: 'nav-settings' },
]

const anonymousEntries: NavEntry[] = [
  { title: 'Регистрация', to: '/register', icon: mdPerson, testId: 'nav-sign-up' },
  { title: 'Вход', to: '/login', icon: mdPassword, testId: 'nav-sign-in' },
]

const bottomEntries = new Map<string, { title: string, icon?: string }>([
  ['/operations', { title: 'Операции' }],
  ['/statistics', { title: 'Статистика' }],
  ['/debts', { title: 'Долги', icon: mdAttachMoney }],
])

export function buildBottomNavigation(): NavItem[] {
  const known = new Map(authorizedEntries
    .flatMap(entry => (isNavGroup(entry) ? entry.items : [entry]))
    .map(item => [item.to, item]))

  return [...bottomEntries].flatMap(([path, entry]) => {
    const item = known.get(path)

    return item === undefined
      ? []
      : [{ ...item, title: entry.title, icon: entry.icon ?? item.icon, testId: `bottom-${item.testId}` }]
  })
}

export function buildNavigation(access: NavAccess): NavEntry[] {
  if (!access.isAuthenticated) {
    return anonymousEntries
  }

  return [...authorizedEntries, profileEntry(access.userName)]
}

export function isNavGroup(entry: NavEntry): entry is NavGroup {
  return 'items' in entry
}

function profileEntry(userName: string | null): NavItem {
  return { title: userName ?? 'Профиль', to: '/profile', icon: mdPerson, testId: 'nav-profile' }
}
