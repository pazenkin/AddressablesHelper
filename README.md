# AddressablesHelper
Небольшой класс для более удобной работы с [адрессаблами Unity](https://docs.unity3d.com/Packages/com.unity.addressables@1.21/manual/index.html).

## Для чего он нужен?
- Работа с адрессаблами через единый интерфейс с помощью нескольких методов без необходимости дублирования кода;
- Загрузка в том числе и одинаковых префабов из разных классов одновременно, вернется и будет сохранен в библиотеке для дальнейшего использования только один экземпляр;
- Возможность повторного использования одних и тех же объектов на разных уровнях и массовой выгрузки из памяти неиспользуемых элементов;
- Все загруженные адрессаблы хранятся в единой библиотеке.

## Как использовать?
1. Для работы необходимо наличие [UniTask](https://github.com/Cysharp/UniTask) в проекте, ассинхронная загрузка адрессаблов осуществляется через него.
2. Далее необходимо разместить [AddressablesHelper.cs](https://github.com/pazenkin/AddressablesHelper/blob/8d01bbe67d4fef3a5744b5d60471aa8f2b09411d/AddressablesHelper.cs) в папке со скриптами проекта.
3. Рекомендуется использовать [Zenject](https://github.com/modesttree/Zenject) для внедрения данного класса везде, где он требуется, поместив MonoInstaller с привязкой класса в ProjectContext.
4. Получив, где требуется, ссылку на класс, использовать его методы.
