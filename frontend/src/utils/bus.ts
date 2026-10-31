import mitt, { Emitter } from 'mitt'

type Events = Record<string, any>

export const emitter: Emitter<Events> = mitt<Events>()
