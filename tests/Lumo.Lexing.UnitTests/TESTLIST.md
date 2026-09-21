# Список тестов

## `LexerErrorTests`

- [x] `Rejects_unexpected_character` — недопустимые символы `@`, `#`, `$`, `~`, `^`, `?`, `\`, `'`, неразрывный пробел, а также одиночные `&` и `|` в вариантах `&`, `&x`, `|` и `|x`
- [x] `Reports_position_of_unexpected_character` — позиция недопустимого символа после перевода строки
- [x] `Includes_position_in_error_message` — позиция включается в текст ошибки
- [x] `Rejects_null_source_text` — конструктор `Lexer` отклоняет `null`

## `LexerIdentifierTests`

- [x] `Can_read_identifier` — однобуквенные идентификаторы, подчёркивания, цифры после первой позиции и длинные имена
- [x] `Treats_identifiers_as_case_sensitive` — `sum`, `Sum`, `SUM`
- [x] `Stops_identifier_on_first_foreign_character` — `abc+def`
- [x] `Rejects_non_latin_letters_in_identifier` — кириллический идентификатор

## `LexerKeywordTests`

- [x] `Can_read_keyword` — все ключевые слова `var`, `func`, `return`, `if`, `else`, `while`, `break`, `continue`, `struct`, `true`, `false`, `int`, `string`, `bool`
- [x] `Reads_word_similar_to_keyword_as_identifier` — другой регистр, расширенные имена, `main` и имена встроенных функций
- [x] `Can_read_keywords_separated_by_punctuation` — ключевые слова рядом со скобками и разделителями

## `LexerIntLiteralTests`

- [x] `Can_read_int_literal` — корректные значения от `0` до `9223372036854775807`
- [x] `Rejects_invalid_int_literal` — ведущие нули, переполнение `long`, буква или `_` сразу после литерала (`12abc`, `0x`, `1_000`, `7_`, `007abc`)
- [x] `Reads_literal_separated_from_identifier` — литерал и идентификатор, разделённые пробелом, оператором или комментарием
- [x] `Reads_minus_before_literal_as_separate_token` — `-5` разбирается как `Minus` и `IntLiteral`

## `LexerStringLiteralTests`

- [x] `Can_read_string_literal` — пустые строки, обычный текст, Unicode, комментарии, пунктуация и escape-последовательности `\n`, `\r`, `\t`, `\"`, `\\` внутри строк, включая перенос `\r\n`
- [x] `Rejects_invalid_string_literal` — незакрытая строка, неэкранированные LF, CR и CR LF, обратная косая черта в конце файла или строки, неизвестные escape-последовательности
- [x] `Can_read_two_literals_in_a_row` — две строковые лексемы подряд
- [x] `Reports_position_of_unterminated_literal` — позиция начала незакрытой строки
- [x] `Reports_position_of_unknown_escape` — позиция обратной косой черты неизвестной escape-последовательности

## `LexerOperatorTests`

- [x] `Can_read_operator` — все одно- и двухсимвольные операторы, разделители и скобки
- [x] `Applies_maximal_munch_rule` — `===`, `= =`, `->`, `- >`, `&&&&`, `!=`, `! =`, `a/b`
- [x] `Can_read_operators_without_spaces` — операторы в выражении `x=(a+b)*c%2;`

## `LexerTriviaTests`

- [x] `Skips_whitespace` — пробел, табуляция, `\n`, `\r`, `\r\n` и их сочетания
- [x] `Skips_comments` — строчные, блочные, многострочные, пустые комментарии и проверка отсутствия вложенности
- [x] `Returns_only_end_of_file_for_empty_text` — пустой текст
- [x] `Returns_only_end_of_file_for_whitespace_and_comments` — текст только из пробелов и комментариев
- [x] `Keeps_token_sequence_around_comments` — комментарии между токенами
- [x] `Separates_tokens_by_comment_without_whitespace` — `a/**/b`
- [x] `Rejects_unterminated_block_comment` — незакрытый блочный комментарий
- [x] `Rejects_block_comment_closed_by_single_star` — `/* * /`

## `LexerPositionTests`

- [x] `Tracks_position_after_line_break` — `\n`, `\r\n`, `\r`, пустые строки и комментарии
- [x] `Tracks_position_of_every_token` — позиции всех токенов в нескольких строках
- [x] `Counts_tab_as_single_column` — табуляция считается одним столбцом
- [x] `Formats_position_as_line_and_column` — `SourcePosition.ToString()`

## `LexerProgramTests`

- [x] `Can_read_minimal_program` — минимальная программа с функцией, переменной и возвратом
- [x] `Can_read_program_with_struct_array_and_loop` — структура, массив, цикл, логические и арифметические операторы, строка и вызов функции
- [x] `Returns_end_of_file_token_repeatedly` — повторный вызов `NextToken` после EOF

## `LexicalErrorExceptionTests`

- [x] `Builds_message_from_description_and_position` — описание, позиция и формат сообщения ошибки

## `TokenTests`

- [x] `Stores_all_parts_of_token` — тип, лексема, позиция, числовое и строковое значения
- [x] `Formats_token_as_position_type_and_lexeme` — `Token.ToString()`

## `SourceFileTests`

- [x] `Can_read_file_into_memory` — чтение UTF-8-файла
- [x] `Skips_byte_order_mark` — обработка BOM
- [x] `Keeps_non_latin_characters` — сохранение Unicode-символов
- [x] `Can_tokenize_text_loaded_from_file` — токенизация текста, прочитанного из файла