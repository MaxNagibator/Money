import type { VueWrapper } from '@vue/test-utils'
import { mount } from '@vue/test-utils'
import { describe, expect, it, vi } from 'vitest'
import type { RouteLocationRaw } from 'vue-router'
import { RouterLink } from 'vue-router'
import MudButton from './MudButton.vue'
import MudFab from './MudFab.vue'
import MudIconButton from './MudIconButton.vue'
import MudLink from './MudLink.vue'

interface LinkProps {
  href?: string
  target?: string
  to?: RouteLocationRaw
  disabled?: boolean
}

const routerStub = { global: { stubs: { RouterLink: { template: '<a><slot /></a>' } } } }

const linkButtons: [string, (props: LinkProps) => VueWrapper][] = [
  ['MudButton', props => mount(MudButton, { props, ...routerStub })],
  ['MudIconButton', props => mount(MudIconButton, { props, ...routerStub })],
]

describe('отключённые кнопки', () => {
  it.each(linkButtons)('%s со ссылкой при disabled остаётся кнопкой', (_name, factory) => {
    const wrapper = factory({ href: 'https://example.com', target: '_blank', disabled: true })

    expect(wrapper.html()).toMatch(/^<button\b/)
    expect(wrapper.attributes('href')).toBeUndefined()
    expect(wrapper.attributes('target')).toBeUndefined()
    expect(wrapper.attributes('disabled')).toBeDefined()
  })

  it.each(linkButtons)('%s с маршрутом при disabled остаётся кнопкой', (_name, factory) => {
    const wrapper = factory({ to: { name: 'home' }, disabled: true })

    expect(wrapper.html()).toMatch(/^<button\b/)
    expect(wrapper.findComponent(RouterLink).exists()).toBe(false)
    expect(wrapper.attributes('disabled')).toBeDefined()
  })

  it.each(linkButtons)('%s без disabled по-прежнему ссылка', (_name, factory) => {
    const wrapper = factory({ href: 'https://example.com' })

    expect(wrapper.html()).toMatch(/^<a\b/)
    expect(wrapper.attributes('href')).toBe('https://example.com')
    expect(wrapper.attributes('disabled')).toBeUndefined()
  })
})

describe('рябь', () => {
  it('MudFab несёт mud-ripple, пока не отключён', () => {
    const wrapper = mount(MudFab)

    expect(wrapper.classes()).toContain('mud-ripple')
    expect(wrapper.attributes('disabled')).toBeUndefined()
  })

  it('отключённый MudFab теряет mud-ripple и получает disabled', () => {
    const wrapper = mount(MudFab, { props: { disabled: true } })

    expect(wrapper.classes()).not.toContain('mud-ripple')
    expect(wrapper.attributes('disabled')).toBeDefined()
  })
})

describe('ссылка', () => {
  it('отключённая ссылка не ведёт никуда и объявляет себя отключённой', () => {
    const wrapper = mount(MudLink, { props: { href: 'https://example.com', disabled: true } })

    expect(wrapper.attributes('href')).toBeUndefined()
    expect(wrapper.attributes('aria-disabled')).toBe('true')
    expect(wrapper.classes()).toContain('mud-link-disabled')
  })

  it('отключённая ссылка с маршрутом не превращается в RouterLink', () => {
    const wrapper = mount(MudLink, { props: { to: { name: 'home' }, disabled: true }, ...routerStub })

    expect(wrapper.findComponent(RouterLink).exists()).toBe(false)
    expect(wrapper.attributes('href')).toBeUndefined()
  })

  it('отключённая ссылка не вызывает обработчик клика', async () => {
    const onClick = vi.fn()
    const wrapper = mount(MudLink, { props: { disabled: true }, attrs: { onClick } })

    await wrapper.trigger('click')

    expect(onClick).not.toHaveBeenCalled()
  })

  it('включённая ссылка вызывает обработчик клика', async () => {
    const onClick = vi.fn()
    const wrapper = mount(MudLink, { attrs: { onClick } })

    await wrapper.trigger('click')

    expect(onClick).toHaveBeenCalledTimes(1)
  })
})

describe('ссылка как кнопка', () => {
  it('ссылка с обработчиком и без адреса получает роль кнопки и курсор', () => {
    const wrapper = mount(MudLink, { attrs: { onClick: () => {} } })

    expect(wrapper.attributes('role')).toBe('button')
    expect(wrapper.classes()).toContain('cursor-pointer')
  })

  it('ссылка с адресом не получает курсор, но роль сохраняет', () => {
    const wrapper = mount(MudLink, { props: { href: 'https://example.com' }, attrs: { onClick: () => {} } })

    expect(wrapper.attributes('role')).toBe('button')
    expect(wrapper.classes()).not.toContain('cursor-pointer')
  })

  it('ссылка без обработчика роли не получает и остаётся при исходном наборе классов', () => {
    const wrapper = mount(MudLink, { props: { href: 'https://example.com' } })

    expect(wrapper.attributes('role')).toBeUndefined()
    expect(wrapper.classes()).toEqual(['mud-typography', 'mud-link', 'mud-primary-text', 'mud-link-underline-hover'])
  })
})
