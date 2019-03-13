# DocTranslate
Program which translates documentation from Russian to English using Yandex.Translate API.

## How to use it
When running the program you should pass these arguments in corresponding order:

1. Working directory (required)

A full path to directory where files are.

2. Yandex.Translator API key (required)

Yandex translator API key can be received for free [here](https://translate.yandex.ru/developers/keys).
Keep in mind that quantity of symbols is limited: 1 000 000 symbols per 24h  and 10 000 000 symbols per month.

3. "force" (required)

You can pass `force` string to turn on force-mode. This way each file will be translated even if it was not modified (by default such files are skipped).

In the header of `.en.md` files there are two params checked by program:

1. `hash`

This is SHA-256 hash of the article in Russian. If article in Russian was not changed ­­— there is no need to re-translate, right?

2. `autotranslate`

If flag set to `true` this page can be translated automatically. If flag set to `false` the article will be skipped by DocTranslate. 

