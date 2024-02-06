# Unity-Storage
Storage for unity behaviours objects

## Установка:
1.Установить пакет Unity Newtonsoft-JSON
2.Скопировать Scripts\Storage в проект
3.Создать Asset **Save System**

## Использование:

Каждая сохраняемая сущность может быть одного из четырех типов:
1. STATIC - Создана в редакторе юнити, сохраняется в рамках сцены, должна иметь уникальное имя
2. GLOBAL_STATIC - Создана в редакторе юнити, сохраняется в рамках игры, должна иметь уникальное имя
3. DYNAMIC - Создана во время игры из префаба (например заспавненный враг). Сохраняется в рамках сцены
4. GLOBAL_DYNAMIC - Создана во время игры из префаба (например заспавненный враг). Будет загружаться на всех сценах.

Скрипт сохранениния каждого GameObject должен быть унаследован от класса **HeroicRealm.SaveableBehaviour**

Скрипт должен реализовать следующие методы:
- string GetPrefabName() - имя префаба для создания сущности. Для сущностей типа STATIC/GLOBAL_STATIC может возвращать пустое значение если префаба нет.
- string SerializeObject() - Возвращает данные сущности в виде строки( JSON/XML/свободный вид)
- void loadData(string packageData) - Загружает данные сущности из строки


Если скрипт присоединяется в редакторе,то должны быть заполнены параметры:
- Save System - Указать ссылку на Asset Save System
- Saveable Type - Указать STATIC или GLOBAL_STATIC

[!CAUTION] Указание других типов значений приведет к ошибкам!!

Так же данные параметры можно установить через методы SetSaveSystem(SaveSystem saveSystem) и SetSaveableType(ESaveableType type).

### Регистрация сущности в системе сохранения

Если у сущности прописаны свойства Save System и Saveable Type, то регистрация пройдет автоматически при вызове метода Awake(),
иначе необходимо заполнить значения этих свойств и вызвать метод RegisterInSaveSystem() базового класса.

[!IMPORTANT] Если скрипт отвечающий за сохранение переопределяет метод Awake() он должен вызвать метод Awake() базового класса или RegisterInSaveSystem()
Пример:
 GameObject pref = Resources.Load("DynamicEnemy") as GameObject;
 GameObject go = GameObject.Instantiate(pref);
 DynamicEnemySaver sb = go.GetComponent<DynamicEnemySaver>();
 sb.SetSavebleType(ESavebleType.DYNAMIC);
 sb.SetSaveSystem(saveSystem); 
 sb.RegisterInSaveSystem();

### Снятие сущности с регистрации в системе сохранения
Если сущность уничтожается, то она автоматически снимается с регистрации.
Для ручного снятия с регистрации вызвать метод UnregisterInSaveSystem().
[!IMPORTANT] Если скрипт отвечающий за сохранение переопределяет метод OnDestroy() он должен вызвать метод OnDestroy() базового класса или метод UnregisterInSaveSystem()

## Сохранение и загрузка
Для сохранения вызвать метод SaveSyste.Save передав ему имя фаила для сохранения.
Для сохранения вызвать метод SaveSyste.Load передав ему имя фаила для сохранения.

Так же можно сохранять данные игры в память, вызвав эти методы  без параметра

## Сохранение данных в свободном виде
Система сохранения позыоляет сохранять любые данные в сериализуемом виде
* Для сохранения данных надо вызвать метод SaveSystem.SetСustomData(String key, object value)
* Для получения данных используется метод GetСustomData(String key)
* Для удаления данных используется метод DeleteCustomData(String key)
* Чтобы проверить есть ли данные, надо вызвать метод ContainsCustomData(String key)


 

