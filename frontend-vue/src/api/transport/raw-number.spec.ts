import { describe, expect, it, vi } from 'vitest'
import { rawNumber, serializeBody } from '@/api/transport/raw-number'

describe('rawNumber', () => {
  it.each([
    ['3.3333333333333333333333333333'],
    ['-0.15'],
    ['0'],
    ['79228162514264337593543950335'],
  ])('принимает десятичный литерал %j', (literal) => {
    expect(serializeBody({ sum: rawNumber(literal) })).toBe(`{"sum":${literal}}`)
  })

  it.each([
    ['1e30'],
    ['1,5'],
    [' 1'],
    ['1.'],
    ['01.5'],
    ['-01'],
    ['abc'],
    [''],
  ])('отвергает %j', (literal) => {
    expect(() => rawNumber(literal)).toThrow(RangeError)
  })
})

describe('serializeBody', () => {
  it('сохраняет все двадцать восемь цифр, которые не выживают в JavaScript-числе', () => {
    const literal = '3.3333333333333333333333333333'

    expect(serializeBody({ sum: rawNumber(literal) })).toBe(`{"sum":${literal}}`)
    expect(JSON.stringify({ sum: Number(literal) })).not.toBe(`{"sum":${literal}}`)
  })

  it('оставляет остальное тело таким же, как обычная сериализация', () => {
    const body = { name: 'Кофе "на вынос"', order: null, tags: [1, 2], nested: { place: 'Кафе\\Бар' } }

    expect(serializeBody(body)).toBe(JSON.stringify(body))
  })

  it('разворачивает каждое сырое число в теле по отдельности', () => {
    const body = { sum: rawNumber('1.005'), paid: rawNumber('0.5'), items: [{ sum: rawNumber('-2.25') }] }

    expect(serializeBody(body)).toBe('{"sum":1.005,"paid":0.5,"items":[{"sum":-2.25}]}')
  })

  it('не трогает пользовательскую строку, похожую на подстановку', () => {
    const body = { comment: 'raw-number:00000000-0000-4000-8000-000000000000', sum: rawNumber('1.5') }

    expect(serializeBody(body)).toBe('{"comment":"raw-number:00000000-0000-4000-8000-000000000000","sum":1.5}')
  })

  it('отказывается подставлять число, когда запрос содержит тот же ключ подстановки', () => {
    const uuid = '00000000-0000-4000-8000-000000000000'

    vi.spyOn(crypto, 'randomUUID').mockReturnValue(uuid)

    const body = { comment: `raw-number:${uuid}`, sum: rawNumber('1.5') }

    expect(() => serializeBody(body)).toThrow('Подстановка числа совпала с содержимым запроса')
  })

  it('форму в полях адреса собирает так же, как openapi-fetch', () => {
    const headers = new Headers({ 'Content-Type': 'application/x-www-form-urlencoded' })

    expect(serializeBody({ grant_type: 'password', username: 'иван' }, headers)).toBe('grant_type=password&username=%D0%B8%D0%B2%D0%B0%D0%BD')
  })

  it('пропускает форму насквозь', () => {
    const form = new FormData()

    expect(serializeBody(form)).toBe(form)
  })

  it('оставляет пустое тело пустым', () => {
    expect(serializeBody(undefined)).toBeUndefined()
  })
})
