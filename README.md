# Инструментация AspNetCore [![Build status](https://ci.appveyor.com/api/projects/status/p8rdj4uu19l3p368/branch/master?svg=true)](https://ci.appveyor.com/project/vostok/log4net/branch/master)

## Свойства контекста запроса
* TraceId
* HttpMethod - GET, POST и т.д.
* Url
* OperationName - Url, из которого убраны параметры. Определяется, например, как Controller+Action
* Host
* StatusCode
* Client ip
* Request.ContentLength
* Response.ContentLength

## Логирование
В контекст логирования обязательно должно попадать поле TraceId.

Запись о начале обработки запроса (уровень INFO):
* Timestamp - метки времени у лог-записи и у начала запроса могут быть разные. Нужно узнать, есть ли такой момент у webapi.
* HttpMethod
* Url
* Headers (дискуссионный вопрос. Есть приватные хедеры, которые не хочется светить в логах. С другой стороны, есть интересные заголовки, например, бюджет времени на операцию. Возможно, нужно настраивать заголовки, которые попадают в лог)
* Client ip
* Request.ContentLength

Запись об окончании обработки запроса (уровень INFO):
* HttpMethod
* Url
* OperationName (если удалось определить)
* StatusCode
* Duration
* Response.ContentLength

В случае возникновения непредвиденного исключения запрос должен заканчиваться кодом 500, и исключение должно писаться в лог с уровнем ERROR.

## Метрики

### Системные метрики

Собираются как извне, так и изнутри приложения - то, что умеет собирать Metrics.NET.
* Memory usage (private bytes, working set, free physical ram)
* CPU usage (load avg, current load per process/total)
* Disk usage (available space, disk queue, read/write bytes)
* Network usage (in/out bytes per minute)
* Thread pool (worker count/max, iocp count/max)
* GC (time, counts: gen0, gen1, gen2)
* Uptime

### Метрики по запросам

Для агрегированных метрик интервал агрегирования должен настраиваться. Для типичного приложения метрики агрегируются за 1 минуту.

Разрезы:
* 0
* OperationName
* Host
* StatusCode
* OperationName/Host
* OperationName/StatusCode
* Host/StatusCode
* Host/StatusCode/OperationName

Метрики:
* Latency
  - min/max
  - p25,50,75,90,95,99
  - median/mean/stddev
* RPM


### Примеры использования

* **Количество обработанных запросов в разрезе по StatusCode**:
  - Отслеживание появления ответов с кодом 500. Код 500 - это всегда неожиданное исключение, т.е ошибка в логике приложения, ошибка программиста.
  - Отслеживание количества ответов с прочими кодами. Например, 503 - это срабатывание механизмов throttling-а - при нормальной нагрузке и правильно настроенной балансировке их не должно быть слишком много - порог должен определяться индивидуально. Еще пример - если количество ошибок 400 становится слишком большим, это может свидетельствовать о том, что что-то случилось с протоколом - клиенты формируют запросы, а сервер не может их разобрать.
* **Количество обработанных запросов в разрезе по Host** - необходимо для мониторинга равномерности балансировки. Все хосты должны работать примерно одинаково.
* **Количество обработанных запросов в разрезе по OperationName** - отслеживание использования поддерживаемой функциональности.
* **Количество обработанных запросов в разрезе по Host/StatusCode** - отслеживание того, что все хосты одинаково хорошо умеют обрабатывать запросы.
* **Количество обработанных запросов в разрезе по Host/OperationName** - отслеживание того, что все операции одинаково хорошо умеют обрабатываться.

Время обработки запросов (latency) - необходимо собирать минимум, максимум, а также 50, 75, 95, 99 перцентили. Желательно, чтобы если запросов не было, не было точки на графике.

* **Latency в разрезе по StatusCode** - имеет смысл мониторить latency по успешно и неуспешно обработанным запросам отдельно.
* **Latency в разрезе по Host/StatusCode** - мониторинг того, что конкретный хост не начинает тупить
* **Latency в разрезе по OperationName/StatusCode** - мониторинг скорости обработки конкретных операций

RPS

Важно не использовать сглаживание (например, экспоненциальное) для этой метрики. Здесь важны точные значения пиков и их продолжительность.

* **RPS в разрезе по Host** - предельный RPS на хост определяется нагрузочным тестированием. По этому параметру настраивается throttling. Приближение к этому порогу сигнализирует о том, что, возможно, пора добавлять реплик.
* **RPS в разрезе по OperationName** - мониторинг нагрузки на различную функциональность сервиса

Для некоторых сервисов имеет смысл собирать все эти метрики дополнительно еще и в разрезе по характерному размеру данных (размер тела запроса, размер тела ответа, размер файла во внешнем хранилище и т.п.). В этом случае необходимо определить диапазоны размеров, по которым надо собирать распределение, исходя из логики предметной области. Имеет смысл делать градацию с экспоненциально растущими длинами диапазонов. Например: до 1Kb, 1Kb-10Kb, 10Kb-100Kb, 100Kb-1Mb, больше 1Mb.
