import {
  mdAssessment,
  mdFavorite,
  mdMoneyOff,
  mdPhoneIphone,
  mdTrendingDown,
} from '@/shared/icons'
import type { ThemeColor } from './version-entry'

export interface PromoCard {
  id: string
  icon: string
  color: ThemeColor
  text: string
}

export const PROMO_TITLE = 'Проект «Филочки»'

export const PROMO_WARNING = 'Этот проект поможет вам контролировать свои финансы и избежать ненужных трат!'

export const PROMO_SOLUTION_TITLE = 'Но решение всех проблем – это проект «Филочки»!'

export const PROMO_CALL_TO_ACTION = 'Время взять под контроль свои деньги и перестать жить в долг!'

export const PROMO_PROBLEMS: readonly PromoCard[] = [
  {
    id: 'mistress',
    icon: mdFavorite,
    color: 'error',
    text: 'Завел очередную любовницу, но не знаешь, сколько на неё тратишь? Каждый вечер в ресторане, подарки на праздники – всё это складывается в круглую сумму, а ты даже не ведёшь учёт.',
  },
  {
    id: 'credit',
    icon: mdPhoneIphone,
    color: 'success',
    text: 'Взял кредит на новый iPhone, но уже забыл про него? Каждый месяц приходит уведомление, а ты всё надеешься, что как-то обойдёшься без лишних трат.',
  },
  {
    id: 'debts',
    icon: mdMoneyOff,
    color: 'warning',
    text: 'Друзья постоянно просят вернуть сотку, а ты у них не занимал? Кажется, что это мелочь, но в итоге накапливается целая куча долгов, и ты начинаешь терять счёт, кому что должен.',
  },
  {
    id: 'income',
    icon: mdTrendingDown,
    color: 'error',
    text: 'Всё это приводит к тому, что доходы начинают таять, а ты даже не понимаешь, куда уходят деньги. Время пересмотреть свои расходы.',
  },
]

export const PROMO_SOLUTION: PromoCard = {
  id: 'assistant',
  icon: mdAssessment,
  color: 'tertiary',
  text: 'Твой личный финансовый помощник позволит не только гибко следить за доходами и расходами, но и поможет планировать бюджет и контролировать свои траты. С помощью «Филочки» ты сможешь легко отслеживать, куда уходят твои деньги, и находить способы сэкономить.',
}
