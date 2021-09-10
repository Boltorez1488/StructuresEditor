![StructuresEditor](https://user-images.githubusercontent.com/89551246/132848082-eb4a2525-2243-4eb2-8e1a-36ea7c73daac.jpg)

# Редактор структур :ru:
Предназначен для генерации дерева структур для C++. Используется в основном в связке:
- Личная сборка Cheat Engine
- [Сканер шаблонов](https://github.com/Boltorez1488/PatternsScanner)
- IDA PRO

*У всего функционала была нужда в создании, ничего лишнего не было создано. Всё, что здесь есть, использовалось реверсерами. Возможно некоторые вещи требуют доработки и модификации. Не поддерживается отмена операций (Ctrl+Z).*

Используемые шрифты:
- Inconsolate
- Roboto
- Showcard Gothic

Функционал:
- Загрузка / Сохранение проектных файлов
- При выходе автоматическое сохранение
- Поддержка x32/x64 полей
- Поддержка Namespace, Struct, Enum
- Несколько уровней вложенности для структур
- Поддержка C++ синтаксиса полей, все типы оформляются в C++ формате
- Встроены базовые типы: int64, int32, int16, int8, uint64, uint32, uint16, uint8
- Автоматический расчёт размера каждого поля
- Автоматический расчёт следующего оффсета во время создания нового поля
- Поддержка массивов
- Поддержка глобального Namespace
- Автоматическая сортировка по имени A-z

## Помощь
```
Меню:
    Shift All Down Offsets - Сдвигать всю нижнюю цепочку оффсетов, при изменении текущего оффсета.
    64 Bit Mode - Переключить в x64 режим. Внимание! Не делает автоматический перерасчёт размеров при изменении,
    поэтому выбрать режим надо сразу перед созданием структур.

Свойства:
    Additonal Include Header File - Путь к дополнительному файлу включения (прим. header.h).
    Empty File Name - Имя рекурсивного файла для перелинковки структур.
    Compiler Output Name - Имя выходного файла со всеми структурами.
    Его надо будет включать уже внутри проекта для доступа к структурам.
    Serializator Path - Путь к файлу конфига для всех структур.
    Compiler Folder - Путь к конечной папке, куда попадают все выходные файлы.
    Global Namespace - Глобальный Namespace, в который будут обёрнуты все структуры.
    Compiler Print Offset - Указывает, что необходимо добавлять в комментарии оффсеты в конечный файл.

Можно использовать колесо мыши на разных полях, также действуют зажатия клавиш Alt || Ctrl || Shift.

Хоткеи:
    Центр:
        Ctrl+S - Сохранить
    Главное окно:
        Ctrl+N - Создать Namespace
        Alt+S - Создать Struct
        Ctrl+E - Создать Enum
        Ctrl+T - Фокусировка на главном окне
        Ctrl+G - Фокусировка на конечном элементе в текстовом поле Path
    Namespace:
        Ctrl+D - Удалить текущий объект
        Ctrl+A - Фокусировка на имени
        Ctrl+N - Создать Namespace
        Alt+S - Создать Struct
        Ctrl+E - Создать Enum
    Struct:
        Ctrl+D - Удалить текущий объект
        Ctrl+A - Фокусировка на имени
        Ctrl+F - Создать простое C++ поле
        Ctrl+P - Создать указатель на структуру
        Alt+S - Создать вложенную структуру
        Ctrl+E - Создать вложенный енум
    Enum:
        Ctrl+D - Удалить текущий объект
        Ctrl+A - Фокусировка на имени
        Ctrl+F - Создать ключ / значение
```

# Structure editor :us:
Designed to generate a tree of structures for C++. Used mainly in conjunction:
- Private build Cheat Engine
- [Patterns Scanner](https://github.com/Boltorez1488/PatternsScanner)
- IDA PRO

*All the functionality needed to be created, nothing superfluous was created. Everything that is here was used by reversers. Some things may need some work and modification. Undoing operations is not supported (Ctrl+Z).*

Using fonts:
- Inconsolate
- Roboto
- Showcard Gothic

Features:
- Loading / Saving Project Files
- Auto save on exit
- Support x32/x64 fields
- Support Namespace, Struct, Enum
- Multiple levels of nesting for structures
- Support for C++ field syntax, all types are formatted in C++ format
- Basic types built in: int64, int32, int16, int8, uint64, uint32, uint16, uint8
- Automatic calculation of the size of each field
- Automatic calculation of the next offset when creating a new field
- Array support
- Support global Namespace
- Automatic sorting by name A-z

## Help
```
Menu:
    Shift All Down Offsets - Shift the entire bottom offset chain when changing the current offset.
    64 Bit Mode - Switch to x64 mode. Attention! Does not automatically recalculate sizes when resizing,
    therefore, you must select the mode immediately before creating the structures.

Properties:
    Additonal Include Header File - The path to the additional include file (ex. header.h).
    Empty File Name - The name of the recursive file for linking structures.
    Compiler Output Name - Output file name with all structures.
    It will need to be included already inside the project to access the structures.
    Serializator Path - The path to the config file for all structures.
    Compiler Folder - The path to the destination folder where all output files go.
    Global Namespace - The global Namespace that all structures will be wrapped in.
    Compiler Print Offset - Indicates that offsets should be added to comments in the final file.

You can use the mouse wheel on different fields, pressing the Alt || Ctrl || Shift keys also works.

Hotkeys:
    Core:
        Ctrl+S - Save
    Main window:
        Ctrl+N - Create Namespace
        Alt+S - Create Struct
        Ctrl+E - Create Enum
        Ctrl+T - Focusing on the main window
        Ctrl+G - Focusing on the end element in the Path text box
    Namespace:
        Ctrl+D - Delete current object
        Ctrl+A - Focus on the name
        Ctrl+N - Create Namespace
        Alt+S - Create Struct
        Ctrl+E - Create Enum
    Struct:
        Ctrl+D - Delete current object
        Ctrl+A - Focus on the name
        Ctrl+F - Create simple C++ field
        Ctrl+P - Create pointer to structure
        Alt+S - Create nested structure
        Ctrl+E - Create nested enum
    Enum:
        Ctrl+D - Delete current object
        Ctrl+A - Focus on the name
        Ctrl+F - Create key / value
```
