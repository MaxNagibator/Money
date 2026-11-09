export interface paths {
    "/Accounts/Register": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Регистрация нового пользователя. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            /** @description Данные пользователя для регистрации. */
            requestBody?: {
                content: {
                    "application/json": components["schemas"]["Money.Api.Dto.Accounts.RegisterAccountRequest"];
                    "text/json": components["schemas"]["Money.Api.Dto.Accounts.RegisterAccountRequest"];
                    "application/*+json": components["schemas"]["Money.Api.Dto.Accounts.RegisterAccountRequest"];
                };
            };
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Accounts/ConfirmEmail": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Подтверждение почты. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            /** @description Данные для подтверждения. */
            requestBody?: {
                content: {
                    "application/json": components["schemas"]["Money.Api.Dto.Accounts.ConfirmEmailRequest"];
                    "text/json": components["schemas"]["Money.Api.Dto.Accounts.ConfirmEmailRequest"];
                    "application/*+json": components["schemas"]["Money.Api.Dto.Accounts.ConfirmEmailRequest"];
                };
            };
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Accounts/ResendConfirmCode": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Повторная отправка кода подтверждения. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Accounts/ChangePassword": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: {
                content: {
                    "application/json": components["schemas"]["Money.Api.Dto.Accounts.ChangePasswordRequest"];
                    "text/json": components["schemas"]["Money.Api.Dto.Accounts.ChangePasswordRequest"];
                    "application/*+json": components["schemas"]["Money.Api.Dto.Accounts.ChangePasswordRequest"];
                };
            };
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/connect/token": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Обменивает учетные данные пользователя на токен доступа.
         * @description Этот метод обрабатывает запросы на получение токена доступа, используя учетные данные пользователя.
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["System.Security.Claims.ClaimsPrincipal"];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/connect/userinfo": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Возвращает информацию о пользователе.
         * @description Этот метод обрабатывает запросы на получение информации о пользователе.
         */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": {
                            [key: string]: string;
                        };
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        /**
         * Возвращает информацию о пользователе.
         * @description Этот метод обрабатывает запросы на получение информации о пользователе.
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": {
                            [key: string]: string;
                        };
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/CarEvents/Car/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Получить список авто-событий. */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор авто. */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Money.Api.Dto.CarEvents.CarEventDto"][];
                        "application/json": components["schemas"]["Money.Api.Dto.CarEvents.CarEventDto"][];
                        "text/json": components["schemas"]["Money.Api.Dto.CarEvents.CarEventDto"][];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/CarEvents/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Получить авто-событие по идентификатору. */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор авто-события. */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Money.Api.Dto.CarEvents.CarEventDto"];
                        "application/json": components["schemas"]["Money.Api.Dto.CarEvents.CarEventDto"];
                        "text/json": components["schemas"]["Money.Api.Dto.CarEvents.CarEventDto"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        /** Обновить авто-события. */
        put: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор обновляемого авто-события. */
                    id: number;
                };
                cookie?: never;
            };
            /** @description Данные для обновления авто-события. */
            requestBody?: {
                content: {
                    "application/json": components["schemas"]["Money.Api.Dto.CarEvents.SaveRequest"];
                    "text/json": components["schemas"]["Money.Api.Dto.CarEvents.SaveRequest"];
                    "application/*+json": components["schemas"]["Money.Api.Dto.CarEvents.SaveRequest"];
                };
            };
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        /** Удалить авто-событие по идентификатору. */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор авто-события. */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/CarEvents": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Создать авто-событие. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            /** @description Данные для создания авто-события. */
            requestBody?: {
                content: {
                    "application/json": components["schemas"]["Money.Api.Dto.CarEvents.SaveRequest"];
                    "text/json": components["schemas"]["Money.Api.Dto.CarEvents.SaveRequest"];
                    "application/*+json": components["schemas"]["Money.Api.Dto.CarEvents.SaveRequest"];
                };
            };
            responses: {
                /** @description Created */
                201: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": number;
                        "application/json": number;
                        "text/json": number;
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/CarEvents/{id}/Restore": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Восстановить авто-событие по идентификатору. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор авто-события. */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Cars": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Получить список авто. */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Money.Api.Dto.Cars.CarDto"][];
                        "application/json": components["schemas"]["Money.Api.Dto.Cars.CarDto"][];
                        "text/json": components["schemas"]["Money.Api.Dto.Cars.CarDto"][];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        /** Создать авто. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            /** @description Данные для создания новой категории. */
            requestBody?: {
                content: {
                    "application/json": components["schemas"]["Money.Api.Dto.Cars.SaveRequest"];
                    "text/json": components["schemas"]["Money.Api.Dto.Cars.SaveRequest"];
                    "application/*+json": components["schemas"]["Money.Api.Dto.Cars.SaveRequest"];
                };
            };
            responses: {
                /** @description Created */
                201: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": number;
                        "application/json": number;
                        "text/json": number;
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Cars/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Получить авто по идентификатору. */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор авто. */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Money.Api.Dto.Cars.CarDto"];
                        "application/json": components["schemas"]["Money.Api.Dto.Cars.CarDto"];
                        "text/json": components["schemas"]["Money.Api.Dto.Cars.CarDto"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        /** Обновить авто. */
        put: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор обновляемой категории. */
                    id: number;
                };
                cookie?: never;
            };
            /** @description Данные для обновления категории. */
            requestBody?: {
                content: {
                    "application/json": components["schemas"]["Money.Api.Dto.Cars.SaveRequest"];
                    "text/json": components["schemas"]["Money.Api.Dto.Cars.SaveRequest"];
                    "application/*+json": components["schemas"]["Money.Api.Dto.Cars.SaveRequest"];
                };
            };
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        /** Удалить авто по идентификатору. */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор категории. */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Cars/{id}/Restore": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Восстановить авто по идентификатору. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор категории. */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Categories": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Получить список категорий операций. */
        get: {
            parameters: {
                query?: {
                    /** @description Тип категории (опционально). */
                    type?: number;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Money.Api.Dto.Categories.CategoryDto"][];
                        "application/json": components["schemas"]["Money.Api.Dto.Categories.CategoryDto"][];
                        "text/json": components["schemas"]["Money.Api.Dto.Categories.CategoryDto"][];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        /** Создать новую категорию операции. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            /** @description Данные для создания новой категории. */
            requestBody?: {
                content: {
                    "application/json": components["schemas"]["Money.Api.Dto.Categories.SaveRequest"];
                    "text/json": components["schemas"]["Money.Api.Dto.Categories.SaveRequest"];
                    "application/*+json": components["schemas"]["Money.Api.Dto.Categories.SaveRequest"];
                };
            };
            responses: {
                /** @description Created */
                201: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": number;
                        "application/json": number;
                        "text/json": number;
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Categories/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Получить категорию операции по идентификатору. */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор категории. */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Money.Api.Dto.Categories.CategoryDto"];
                        "application/json": components["schemas"]["Money.Api.Dto.Categories.CategoryDto"];
                        "text/json": components["schemas"]["Money.Api.Dto.Categories.CategoryDto"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        /** Обновить существующую категорию операции. */
        put: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор обновляемой категории. */
                    id: number;
                };
                cookie?: never;
            };
            /** @description Данные для обновления категории. */
            requestBody?: {
                content: {
                    "application/json": components["schemas"]["Money.Api.Dto.Categories.SaveRequest"];
                    "text/json": components["schemas"]["Money.Api.Dto.Categories.SaveRequest"];
                    "application/*+json": components["schemas"]["Money.Api.Dto.Categories.SaveRequest"];
                };
            };
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        /** Удалить категорию операции по идентификатору. */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор категории. */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Categories/{id}/Restore": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Восстановить категорию операции по идентификатору. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор категории. */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/LoadDefault/{isAdd}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Восстановить категории по умолчанию. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /**
                     * @description Флаг, указывающий, добавлять ли категории по умолчанию (по умолчанию true).
                     *     <c>
                     *     Если значение false, все добавленные пользователем категории будут удалены без возможности восстановления!
                     *     </c>
                     */
                    isAdd: boolean;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Debts": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Получить список долгов. */
        get: {
            parameters: {
                query?: {
                    /** @description Включить погашенные долги в результат. */
                    withPaid?: boolean;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Money.Api.Dto.Debts.DebtDto"][];
                        "application/json": components["schemas"]["Money.Api.Dto.Debts.DebtDto"][];
                        "text/json": components["schemas"]["Money.Api.Dto.Debts.DebtDto"][];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        /** Создать долг. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            /** @description Данные для создания долга. */
            requestBody?: {
                content: {
                    "application/json": components["schemas"]["Money.Api.Dto.Debts.SaveRequest"];
                    "text/json": components["schemas"]["Money.Api.Dto.Debts.SaveRequest"];
                    "application/*+json": components["schemas"]["Money.Api.Dto.Debts.SaveRequest"];
                };
            };
            responses: {
                /** @description Created */
                201: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": number;
                        "application/json": number;
                        "text/json": number;
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Debts/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Получить долг по идентификатору. */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор. */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Money.Api.Dto.Debts.DebtDto"];
                        "application/json": components["schemas"]["Money.Api.Dto.Debts.DebtDto"];
                        "text/json": components["schemas"]["Money.Api.Dto.Debts.DebtDto"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        /** Обновить долг. */
        put: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор. */
                    id: number;
                };
                cookie?: never;
            };
            /** @description Данные для обновления долга. */
            requestBody?: {
                content: {
                    "application/json": components["schemas"]["Money.Api.Dto.Debts.SaveRequest"];
                    "text/json": components["schemas"]["Money.Api.Dto.Debts.SaveRequest"];
                    "application/*+json": components["schemas"]["Money.Api.Dto.Debts.SaveRequest"];
                };
            };
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        /** Удалить долг по идентификатору. */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор. */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Debts/{id}/Restore": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Восстановить долг по идентификатору. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор. */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Debts/{id}/Pay": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Оплатить долг. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор. */
                    id: number;
                };
                cookie?: never;
            };
            /** @description Данные для обновления операции. */
            requestBody?: {
                content: {
                    "application/json": components["schemas"]["Money.Api.Dto.Debts.PayRequest"];
                    "text/json": components["schemas"]["Money.Api.Dto.Debts.PayRequest"];
                    "application/*+json": components["schemas"]["Money.Api.Dto.Debts.PayRequest"];
                };
            };
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Debts/MergeOwners/{fromUserId}/with/{toUserId}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Перенести все долги из одного держателя в другого. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор сливаемого должника. */
                    fromUserId: number;
                    /** @description Идентификатор поглощающего должника. */
                    toUserId: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Debts/Owners": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Получить список держателей долга. */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Money.Api.Dto.Debts.DebtOwnerDto"][];
                        "application/json": components["schemas"]["Money.Api.Dto.Debts.DebtOwnerDto"][];
                        "text/json": components["schemas"]["Money.Api.Dto.Debts.DebtOwnerDto"][];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Debts/Owners/{offset}/{count}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Получить список держателей долга с фильтрацией по имени. */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Сдвиг. */
                    offset: number;
                    /** @description Количество. */
                    count: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": string[];
                        "application/json": string[];
                        "text/json": string[];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Debts/Owners/{offset}/{count}/{name}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Получить список держателей долга с фильтрацией по имени. */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Сдвиг. */
                    offset: number;
                    /** @description Количество. */
                    count: number;
                    /** @description Необязательный фильтр по имени. */
                    name: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": string[];
                        "application/json": string[];
                        "text/json": string[];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Debts/Forgive": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Простить долг. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            /** @description Данные для прощения долга. */
            requestBody?: {
                content: {
                    "application/json": components["schemas"]["Money.Api.Dto.Debts.ForgiveRequest"];
                    "text/json": components["schemas"]["Money.Api.Dto.Debts.ForgiveRequest"];
                    "application/*+json": components["schemas"]["Money.Api.Dto.Debts.ForgiveRequest"];
                };
            };
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Development/LoadTestData": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Восстанавливает все данные пользователя, удаляя существующие категории, операции и места,
         *     а затем добавляя новые.
         */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/external/login/auth": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: {
            parameters: {
                query?: {
                    returnUrl?: string;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/external/login/github": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: {
            parameters: {
                query?: {
                    returnUrl?: string;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/connect/callback": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/external/denied": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/FastOperations": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Получить список быстрых операций. */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Money.Api.Dto.FastOperations.FastOperationDto"][];
                        "application/json": components["schemas"]["Money.Api.Dto.FastOperations.FastOperationDto"][];
                        "text/json": components["schemas"]["Money.Api.Dto.FastOperations.FastOperationDto"][];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        /** Создать новую быструю операцию. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            /** @description Данные для создания новой быстрой операции. */
            requestBody?: {
                content: {
                    "application/json": components["schemas"]["Money.Api.Dto.FastOperations.SaveRequest"];
                    "text/json": components["schemas"]["Money.Api.Dto.FastOperations.SaveRequest"];
                    "application/*+json": components["schemas"]["Money.Api.Dto.FastOperations.SaveRequest"];
                };
            };
            responses: {
                /** @description Created */
                201: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": number;
                        "application/json": number;
                        "text/json": number;
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/FastOperations/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Получить быструю операцию по идентификатору. */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор. */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Money.Api.Dto.FastOperations.FastOperationDto"];
                        "application/json": components["schemas"]["Money.Api.Dto.FastOperations.FastOperationDto"];
                        "text/json": components["schemas"]["Money.Api.Dto.FastOperations.FastOperationDto"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        /** Обновить существующую быструю операцию. */
        put: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор операции. */
                    id: number;
                };
                cookie?: never;
            };
            /** @description Данные для обновления операции. */
            requestBody?: {
                content: {
                    "application/json": components["schemas"]["Money.Api.Dto.FastOperations.SaveRequest"];
                    "text/json": components["schemas"]["Money.Api.Dto.FastOperations.SaveRequest"];
                    "application/*+json": components["schemas"]["Money.Api.Dto.FastOperations.SaveRequest"];
                };
            };
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        /** Удалить быструю операцию по идентификатору. */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор категории. */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/FastOperations/{id}/Restore": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Восстановить быструю операцию по идентификатору. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор категории. */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Files": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Загрузить файл. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: {
                content: {
                    "multipart/form-data": {
                        /**
                         * Format: binary
                         * @description Файл для загрузки.
                         */
                        file?: string;
                    };
                };
            };
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Money.Api.Dto.Files.FileDto"];
                        "application/json": components["schemas"]["Money.Api.Dto.Files.FileDto"];
                        "text/json": components["schemas"]["Money.Api.Dto.Files.FileDto"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Operations": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Получить список операций. */
        get: {
            parameters: {
                query?: {
                    /** @description Дата начала периода. */
                    DateFrom?: string;
                    /** @description Дата окончания периода. */
                    DateTo?: string;
                    /** @description Список идентификаторов категорий (цифры через запятую. Пример: "1,2,5"). */
                    CategoryIds?: string;
                    /** @description Комментарий. */
                    Comment?: string;
                    /** @description Место. */
                    Place?: string;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Money.Api.Dto.Operations.OperationDto"][];
                        "application/json": components["schemas"]["Money.Api.Dto.Operations.OperationDto"][];
                        "text/json": components["schemas"]["Money.Api.Dto.Operations.OperationDto"][];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        /** Создать новую операцию. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            /** @description Данные для создания новой операции. */
            requestBody?: {
                content: {
                    "application/json": components["schemas"]["Money.Api.Dto.Operations.SaveRequest"];
                    "text/json": components["schemas"]["Money.Api.Dto.Operations.SaveRequest"];
                    "application/*+json": components["schemas"]["Money.Api.Dto.Operations.SaveRequest"];
                };
            };
            responses: {
                /** @description Created */
                201: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": number;
                        "application/json": number;
                        "text/json": number;
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Operations/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Получить операцию по идентификатору. */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор. */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Money.Api.Dto.Operations.OperationDto"];
                        "application/json": components["schemas"]["Money.Api.Dto.Operations.OperationDto"];
                        "text/json": components["schemas"]["Money.Api.Dto.Operations.OperationDto"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        /** Обновить существующую операцию. */
        put: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор операции. */
                    id: number;
                };
                cookie?: never;
            };
            /** @description Данные для обновления операции. */
            requestBody?: {
                content: {
                    "application/json": components["schemas"]["Money.Api.Dto.Operations.SaveRequest"];
                    "text/json": components["schemas"]["Money.Api.Dto.Operations.SaveRequest"];
                    "application/*+json": components["schemas"]["Money.Api.Dto.Operations.SaveRequest"];
                };
            };
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        /** Удалить категорию операции по идентификатору. */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор категории. */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Operations/UpdateBatch": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Обновить категорию у операций. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            /** @description Данные для обновления операций. */
            requestBody?: {
                content: {
                    "application/json": components["schemas"]["Money.Api.Dto.Operations.UpdateOperationsBatchRequest"];
                    "text/json": components["schemas"]["Money.Api.Dto.Operations.UpdateOperationsBatchRequest"];
                    "application/*+json": components["schemas"]["Money.Api.Dto.Operations.UpdateOperationsBatchRequest"];
                };
            };
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Money.Api.Dto.Operations.OperationDto"][];
                        "application/json": components["schemas"]["Money.Api.Dto.Operations.OperationDto"][];
                        "text/json": components["schemas"]["Money.Api.Dto.Operations.OperationDto"][];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Operations/{id}/Restore": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Восстановить категорию операции по идентификатору. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор категории. */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Operations/GetPlaces/{offset}/{count}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Получить список мест на основе указанного сдвига, количества и необязательной фильтрации по имени. */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Сдвиг. */
                    offset: number;
                    /** @description Количество. */
                    count: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": string[];
                        "application/json": string[];
                        "text/json": string[];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Operations/GetPlaces/{offset}/{count}/{name}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Получить список мест на основе указанного сдвига, количества и необязательной фильтрации по имени. */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Сдвиг. */
                    offset: number;
                    /** @description Количество. */
                    count: number;
                    /** @description Необязательный фильтр по имени. */
                    name: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": string[];
                        "application/json": string[];
                        "text/json": string[];
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Operations/InferCategory/{place}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Подсказать категорию по имени места на основе последних операций пользователя. */
        get: {
            parameters: {
                query?: {
                    /** @description Тип операции, для которого нужна подсказка. */
                    operationType?: components["schemas"]["Money.Business.Enums.OperationTypes"];
                    /** @description Сколько последних операций учитывать. */
                    count?: number;
                };
                header?: never;
                path: {
                    /** @description Имя места (точное совпадение). */
                    place: string;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Money.Api.Dto.Operations.InferCategoryResponseDto"];
                        "application/json": components["schemas"]["Money.Api.Dto.Operations.InferCategoryResponseDto"];
                        "text/json": components["schemas"]["Money.Api.Dto.Operations.InferCategoryResponseDto"];
                    };
                };
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/Operations/ExcelFile": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get: {
            parameters: {
                query?: {
                    /** @description Дата начала периода. */
                    DateFrom?: string;
                    /** @description Дата окончания периода. */
                    DateTo?: string;
                    /** @description Список идентификаторов категорий (цифры через запятую. Пример: "1,2,5"). */
                    CategoryIds?: string;
                    /** @description Комментарий. */
                    Comment?: string;
                    /** @description Место. */
                    Place?: string;
                };
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
            };
        };
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/RegularOperations": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Получить список быстрых операций. */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Money.Api.Dto.RegularOperations.RegularOperationDto"][];
                        "application/json": components["schemas"]["Money.Api.Dto.RegularOperations.RegularOperationDto"][];
                        "text/json": components["schemas"]["Money.Api.Dto.RegularOperations.RegularOperationDto"][];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        put?: never;
        /** Создать новую быструю операцию. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path?: never;
                cookie?: never;
            };
            /** @description Данные для создания новой быстрой операции. */
            requestBody?: {
                content: {
                    "application/json": components["schemas"]["Money.Api.Dto.RegularOperations.SaveRequest"];
                    "text/json": components["schemas"]["Money.Api.Dto.RegularOperations.SaveRequest"];
                    "application/*+json": components["schemas"]["Money.Api.Dto.RegularOperations.SaveRequest"];
                };
            };
            responses: {
                /** @description Created */
                201: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": number;
                        "application/json": number;
                        "text/json": number;
                    };
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/RegularOperations/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Получить быструю операцию по идентификатору. */
        get: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор. */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description OK */
                200: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Money.Api.Dto.RegularOperations.RegularOperationDto"];
                        "application/json": components["schemas"]["Money.Api.Dto.RegularOperations.RegularOperationDto"];
                        "text/json": components["schemas"]["Money.Api.Dto.RegularOperations.RegularOperationDto"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        /** Обновить существующую быструю операцию. */
        put: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор операции. */
                    id: number;
                };
                cookie?: never;
            };
            /** @description Данные для обновления операции. */
            requestBody?: {
                content: {
                    "application/json": components["schemas"]["Money.Api.Dto.RegularOperations.SaveRequest"];
                    "text/json": components["schemas"]["Money.Api.Dto.RegularOperations.SaveRequest"];
                    "application/*+json": components["schemas"]["Money.Api.Dto.RegularOperations.SaveRequest"];
                };
            };
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        post?: never;
        /** Удалить быструю операцию по идентификатору. */
        delete: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор категории. */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/RegularOperations/{id}/Restore": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Восстановить быструю операцию по идентификатору. */
        post: {
            parameters: {
                query?: never;
                header?: never;
                path: {
                    /** @description Идентификатор категории. */
                    id: number;
                };
                cookie?: never;
            };
            requestBody?: never;
            responses: {
                /** @description No Content */
                204: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content?: never;
                };
                /** @description Bad Request */
                400: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Unauthorized */
                401: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
                /** @description Not Found */
                404: {
                    headers: {
                        [name: string]: unknown;
                    };
                    content: {
                        "text/plain": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "application/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                        "text/json": components["schemas"]["Microsoft.AspNetCore.Mvc.ProblemDetails"];
                    };
                };
            };
        };
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
}
export type webhooks = Record<string, never>;
export interface components {
    schemas: {
        "Microsoft.AspNetCore.Mvc.ProblemDetails": {
            type?: string | null;
            title?: string | null;
            /** Format: int32 */
            status?: number | null;
            detail?: string | null;
            instance?: string | null;
        } & {
            [key: string]: unknown;
        };
        "Money.Api.Dto.Accounts.ChangePasswordRequest": {
            currentPassword: string | null;
            newPassword: string | null;
        };
        /** @description Запрос на подтверждение электронной почты. */
        "Money.Api.Dto.Accounts.ConfirmEmailRequest": {
            /** @description Код подтверждения. */
            confirmCode: string | null;
        };
        /** @description Запрос на регистрацию нового аккаунта. */
        "Money.Api.Dto.Accounts.RegisterAccountRequest": {
            /** @description Имя пользователя. */
            userName: string | null;
            /** @description Адрес электронной почты пользователя. */
            email?: string | null;
            /** @description Пароль пользователя. */
            password: string | null;
        };
        /** @description Авто-событие. */
        "Money.Api.Dto.CarEvents.CarEventDto": {
            /**
             * Format: int32
             * @description Идентификатор.
             */
            id?: number;
            /** @description Название. */
            title?: string | null;
            /**
             * Format: int32
             * @description Идентификатор типа.
             */
            typeId?: number;
            /** @description Дополнительные комментарии. */
            comment?: string | null;
            /**
             * Format: int32
             * @description Пробег автомобиля.
             */
            mileage?: number | null;
            /**
             * Format: date-time
             * @description Дата.
             */
            date?: string;
        };
        /** @description Запрос на сохранение авто-события. */
        "Money.Api.Dto.CarEvents.SaveRequest": {
            /** @description Название. */
            title?: string | null;
            /**
             * Format: int32
             * @description Идентификатор типа.
             */
            typeId?: number;
            /**
             * Format: int32
             * @description Идентификатор связанного автомобиля.
             */
            carId?: number;
            /** @description Дополнительные комментарии. */
            comment?: string | null;
            /**
             * Format: int32
             * @description Пробег автомобиля.
             */
            mileage?: number | null;
            /**
             * Format: date-time
             * @description Дата.
             */
            date?: string;
        };
        /** @description Авто. */
        "Money.Api.Dto.Cars.CarDto": {
            /**
             * Format: int32
             * @description Идентификатор.
             */
            id?: number;
            /** @description Наименование. */
            name: string | null;
        };
        /** @description Запрос на сохранение авто. */
        "Money.Api.Dto.Cars.SaveRequest": {
            /** @description Название. */
            name: string | null;
        };
        /** @description Категория операции. */
        "Money.Api.Dto.Categories.CategoryDto": {
            /**
             * Format: int32
             * @description Идентификатор.
             */
            id?: number;
            /** @description Наименование. */
            name: string | null;
            /**
             * Format: int32
             * @description Идентификатор родительской категории (если есть).
             */
            parentId?: number | null;
            /**
             * Format: int32
             * @description Порядок отображения.
             */
            order?: number | null;
            /** @description Цвет. */
            color?: string | null;
            /**
             * Format: int32
             * @description Идентификатор типа операции.
             */
            operationTypeId: number;
        };
        /** @description Запрос на сохранение категории операции. */
        "Money.Api.Dto.Categories.SaveRequest": {
            /** @description Цвет категории операции. */
            color?: string | null;
            /**
             * Format: int32
             * @description Идентификатор родительской категории (если есть).
             */
            parentId?: number | null;
            /** @description Название категории операции. */
            name: string | null;
            /**
             * Format: int32
             * @description Порядок отображения категории.
             */
            order?: number | null;
            /** @description Описание категории операции. */
            description?: string | null;
            /**
             * Format: int32
             * @description Идентификатор типа операции.
             */
            operationTypeId: number;
        };
        /** @description Долг. */
        "Money.Api.Dto.Debts.DebtDto": {
            /**
             * Format: int32
             * @description Идентификатор.
             */
            id?: number;
            /**
             * Format: int32
             * @description Идентификатор типа.
             */
            typeId?: number;
            /**
             * Format: double
             * @description Сумма.
             */
            sum?: number;
            /** @description Комментарий. */
            comment?: string | null;
            /** @description Имя держателя. */
            ownerName: string | null;
            /**
             * Format: date-time
             * @description Дата возникновения.
             */
            date?: string;
            /**
             * Format: double
             * @description Оплаченная сумма.
             */
            paySum?: number;
            /** @description Комментарии к оплате. */
            payComment?: string | null;
            /** @description Удален. */
            isDeleted?: boolean;
        };
        /** @description Держатель долга. */
        "Money.Api.Dto.Debts.DebtOwnerDto": {
            /**
             * Format: int32
             * @description Идентификатор.
             */
            id?: number;
            /** @description Наименование. */
            name: string | null;
        };
        /** @description Простить долг. */
        "Money.Api.Dto.Debts.ForgiveRequest": {
            /** @description Идентификаторы долгов. */
            debtIds: number[] | null;
            /**
             * Format: int32
             * @description Идентификатор категории расходов.
             */
            operationCategoryId: number;
            /** @description Комментарий в расходах. */
            operationComment?: string | null;
        };
        /** @description Запрос на оплату долга. */
        "Money.Api.Dto.Debts.PayRequest": {
            /**
             * Format: double
             * @description Сумма.
             */
            sum: number;
            /** @description Комментарий. */
            comment?: string | null;
            /**
             * Format: date-time
             * @description Дата.
             */
            date: string;
        };
        /** @description Запрос на сохранение долга. */
        "Money.Api.Dto.Debts.SaveRequest": {
            /**
             * Format: int32
             * @description Идентификатор типа.
             */
            typeId: number;
            /**
             * Format: double
             * @description Сумма.
             */
            sum: number;
            /** @description Комментарий. */
            comment?: string | null;
            /** @description Имя владельца, которому принадлежит. */
            ownerName: string | null;
            /**
             * Format: date-time
             * @description Дата возникновения.
             */
            date: string;
        };
        /** @description Быстрая операция. */
        "Money.Api.Dto.FastOperations.FastOperationDto": {
            /**
             * Format: int32
             * @description Идентификатор.
             */
            id?: number;
            /** @description Наименование. */
            name: string | null;
            /**
             * Format: int32
             * @description Порядок сортировки.
             */
            order?: number | null;
            /**
             * Format: int32
             * @description Идентификатор категории.
             */
            categoryId: number;
            /**
             * Format: double
             * @description Сумма.
             */
            sum?: number;
            /** @description Комментарий. */
            comment?: string | null;
            /** @description Место. */
            place?: string | null;
        };
        /** @description Запрос на сохранение операции. */
        "Money.Api.Dto.FastOperations.SaveRequest": {
            /**
             * Format: int32
             * @description Идентификатор категории.
             */
            categoryId: number;
            /**
             * Format: double
             * @description Сумма.
             */
            sum: number;
            /** @description Комментарий. */
            comment?: string | null;
            /** @description Место. */
            place?: string | null;
            /** @description Наименование. */
            name: string | null;
            /**
             * Format: int32
             * @description Значение сортировки.
             */
            order?: number | null;
        };
        /** @description Файл. */
        "Money.Api.Dto.Files.FileDto": {
            /** @description Название файла. */
            fileName?: string | null;
            fileType?: components["schemas"]["Money.Business.Enums.FileTypes"];
        };
        /** @description Ответ с подсказкой категории по месту. */
        "Money.Api.Dto.Operations.InferCategoryResponseDto": {
            /**
             * Format: int32
             * @description Идентификатор подсказанной категории.
             */
            categoryId: number;
        };
        /** @description Операция. */
        "Money.Api.Dto.Operations.OperationDto": {
            /**
             * Format: int32
             * @description Идентификатор.
             */
            id?: number;
            /**
             * Format: int32
             * @description Идентификатор категории.
             */
            categoryId: number;
            /**
             * Format: double
             * @description Сумма.
             */
            sum?: number;
            /** @description Комментарий. */
            comment?: string | null;
            /** @description Место. */
            place?: string | null;
            /**
             * Format: date-time
             * @description Дата.
             */
            date?: string;
            /**
             * Format: int32
             * @description Идентификатор родительской регулярной задачи.
             */
            createdTaskId?: number | null;
        };
        /** @description Запрос на сохранение операции. */
        "Money.Api.Dto.Operations.SaveRequest": {
            /**
             * Format: int32
             * @description Идентификатор категории.
             */
            categoryId: number;
            /**
             * Format: double
             * @description Сумма.
             */
            sum: number;
            /** @description Комментарий. */
            comment?: string | null;
            /** @description Место. */
            place?: string | null;
            /**
             * Format: date-time
             * @description Дата.
             */
            date: string;
        };
        /** @description Запрос на обновление пакета операций. */
        "Money.Api.Dto.Operations.UpdateOperationsBatchRequest": {
            /** @description Список идентификаторов, которые необходимо обновить. */
            operationIds: number[] | null;
            /**
             * Format: int32
             * @description Идентификатор категории, в которую необходимо переместить операции.
             */
            categoryId: number;
        };
        /** @description Регулярная операция. */
        "Money.Api.Dto.RegularOperations.RegularOperationDto": {
            /**
             * Format: int32
             * @description Идентификатор.
             */
            id?: number;
            /** @description Наименование. */
            name: string | null;
            /**
             * Format: int32
             * @description Идентификатор категории.
             */
            categoryId: number;
            /**
             * Format: double
             * @description Сумма.
             */
            sum?: number;
            /** @description Комментарий. */
            comment?: string | null;
            /** @description Место. */
            place?: string | null;
            /**
             * Format: int32
             * @description Идентификатор типа временного шага.
             */
            timeTypeId?: number;
            /**
             * Format: int32
             * @description Значение временного шага.
             */
            timeValue?: number | null;
            /**
             * Format: date-time
             * @description Дата начала.
             */
            dateFrom?: string;
            /**
             * Format: date-time
             * @description Дата окончания.
             */
            dateTo?: string | null;
            /**
             * Format: date-time
             * @description Время последнего запуска.
             */
            runTime?: string | null;
        };
        /** @description Запрос на сохранение операции. */
        "Money.Api.Dto.RegularOperations.SaveRequest": {
            /**
             * Format: int32
             * @description Идентификатор категории.
             */
            categoryId: number;
            /**
             * Format: double
             * @description Сумма.
             */
            sum: number;
            /** @description Комментарий. */
            comment?: string | null;
            /** @description Место. */
            place?: string | null;
            /** @description Наименование. */
            name: string | null;
            /**
             * Format: int32
             * @description Идентификатор типа временного шага.
             */
            timeTypeId: number;
            /**
             * Format: int32
             * @description Значение временного шага.
             */
            timeValue?: number | null;
            /**
             * Format: date-time
             * @description Дата начала.
             */
            dateFrom: string;
            /**
             * Format: date-time
             * @description Дата окончания.
             */
            dateTo?: string | null;
        };
        /**
         * Format: int32
         * @enum {integer}
         */
        "Money.Business.Enums.FileTypes": 1 | 2;
        /**
         * Format: int32
         * @enum {integer}
         */
        "Money.Business.Enums.OperationTypes": 1 | 2;
        "System.Security.Claims.Claim": {
            readonly issuer?: string | null;
            readonly originalIssuer?: string | null;
            readonly properties?: {
                [key: string]: string;
            } | null;
            subject?: components["schemas"]["System.Security.Claims.ClaimsIdentity"];
            readonly type?: string | null;
            readonly value?: string | null;
            readonly valueType?: string | null;
        };
        "System.Security.Claims.ClaimsIdentity": {
            readonly authenticationType?: string | null;
            readonly isAuthenticated?: boolean;
            actor?: components["schemas"]["System.Security.Claims.ClaimsIdentity"];
            bootstrapContext?: unknown;
            readonly claims?: components["schemas"]["System.Security.Claims.Claim"][] | null;
            label?: string | null;
            readonly name?: string | null;
            readonly nameClaimType?: string | null;
            readonly roleClaimType?: string | null;
        };
        "System.Security.Claims.ClaimsPrincipal": {
            readonly claims?: components["schemas"]["System.Security.Claims.Claim"][] | null;
            readonly identities?: components["schemas"]["System.Security.Claims.ClaimsIdentity"][] | null;
            identity?: components["schemas"]["System.Security.Principal.IIdentity"];
        };
        "System.Security.Principal.IIdentity": {
            readonly name?: string | null;
            readonly authenticationType?: string | null;
            readonly isAuthenticated?: boolean;
        };
    };
    responses: never;
    parameters: never;
    requestBodies: never;
    headers: never;
    pathItems: never;
}
export type $defs = Record<string, never>;
export type operations = Record<string, never>;
