using System.Data;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Calculator;

/// <summary>
/// Главное окно приложения
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Конструктор главного окна
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        PreviewKeyDown += MainWindow_PreviewKeyDown;
    }
    
    /// <summary>
    /// Заданная допустимая погрешность вычислений
    /// </summary>
    private double epsilon = 0.000000000000001;

    /// <summary>
    /// Текущее значение
    /// </summary>
    private string currentValue = string.Empty;

    /// <summary>
    /// Список значений для вычислений
    /// </summary>
    private List<string> valuesList = new List<string>();

    /// <summary>
    /// История операций
    /// </summary>
    private StringBuilder operationHistory = new StringBuilder();

    /// <summary>
    /// Флаг, указывающий на то, что результат был вычислен
    /// </summary>
    private bool resultCalculated = false;

    /// <summary>
    /// Флаг, указывающий на использование расширенного режима
    /// </summary>
    private bool isExtendedMode = false;

    /// <summary>
    /// Флаг, указывающий на использование градусного режима для тригонометрических функций
    /// </summary>
    private bool isDegreeMode = false; 

    /// <summary>
    /// Обработчик нажатия кнопок
    /// </summary>
    private void Button_Click(object sender, RoutedEventArgs rea)
    {
        Button button = (Button)sender;
        string buttonText = button.Content.ToString();
        
        if (buttonText == "C")
        {
            ClearAll();
            return;
        }
        
        if (displayTextBox.Text == "Ошибка")
        {
            HandleErrorInput(buttonText);
            return;
        }
        
        if (IsNumeric(buttonText) || buttonText == CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
        {
            HandleNumericInput(buttonText);
            return;
        }
        
        switch (buttonText) 
        {
            case "+":
            case "-":
            case "*":
            case "/":
                HandleOperationInput(buttonText);
                break;
            case "x^2":
                Squaring();
                break;
            case "=":
                HandleEqualsInput();
                break;
            case "+/-":
                ToggleSign();
                break;
            case ",":
                if (!currentValue.Contains(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                {
                    currentValue += buttonText;
                    UpdateDisplay();
                }
                break;
            case "e":
                SetEulerNumber();
                break;
        }
    }
    
    /// <summary>
    /// Метод для очистки всех полей и истории операций
    /// </summary>
    private void ClearAll()
    {
        currentValue = "0";
        displayTextBox.Text = "0";
        valuesList.Clear();
        operationHistory.Clear();
        UpdateOperationHistory();
    }
    
    /// <summary>
    /// Метод для обработки ввода при отображении ошибки
    /// </summary>
    private void HandleErrorInput(string buttonText)
    {
        if (IsNumeric(buttonText) || buttonText == CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
        {
            currentValue = buttonText;
            displayTextBox.Text = currentValue;
            operationHistory.Clear();
            UpdateDisplay();
            UpdateOperationHistory();
        }
    }
    
    /// <summary>
    /// Метод для обработки ввода чисел и десятичного разделителя
    /// </summary>
    private void HandleNumericInput(string buttonText)
    {
        string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        if (buttonText == decimalSeparator && !currentValue.Contains(decimalSeparator))
        {
            if (string.IsNullOrEmpty(currentValue))
            {
                currentValue = "0" + buttonText;
            }
            else
            {
                currentValue += buttonText;
            }
        }
        else if (buttonText != decimalSeparator)
        {
            if (currentValue == "0")
            {
                currentValue = buttonText;
            }
            else
            {
                currentValue += buttonText;
            }
        }

        // Удаление ведущих нулей
        if (currentValue.Length > 1 && currentValue[0] == '0' && currentValue[1] != decimalSeparator[0])
        {
            currentValue = currentValue.TrimStart('0');
        }

        if (string.IsNullOrEmpty(currentValue))
        {
            currentValue = "0";
        }

        UpdateDisplay();
    }

    /// <summary>
    /// Метод для обработки ввода операций
    /// </summary>
    private void HandleOperationInput(string buttonText)
    {
        if (!string.IsNullOrEmpty(currentValue))
        {
            if (resultCalculated)
            {
                operationHistory.Clear();
                resultCalculated = false;
            }

            if (operationHistory.Length > 0)
            {
                operationHistory.Append(" " + currentValue + " " + buttonText);
            }
            else
            {
                operationHistory.Append(currentValue + " " + buttonText);
            }

            valuesList.Add(currentValue);
            valuesList.Add(buttonText);
            currentValue = string.Empty;
            UpdateOperationHistory();
        }
    }
    
    /// <summary>
    /// Метод для обработки ввода "="
    /// </summary>
    private void HandleEqualsInput()
    {
        if (!string.IsNullOrEmpty(currentValue))
        {
            valuesList.Add(currentValue);
            double result = ProcessValues();

            if (operationHistory.Length > 0)
            {
                operationHistory.Append(" " + currentValue + " =");
            }
            else
            {
                operationHistory.Append(result + " =");
            }

            currentValue = result.ToString();
            valuesList.Clear();
            resultCalculated = true;
            UpdateDisplay();
            UpdateOperationHistory();
        }
    }
    
    /// <summary>
    /// Метод для изменения знака числа
    /// </summary>
    private void ToggleSign()
    {
        if (!string.IsNullOrEmpty(currentValue) && currentValue != "0")
        {
            if (currentValue[0] == '-')
            {
                currentValue = currentValue.Substring(1);
            }
            else
            {
                currentValue = "-" + currentValue;
            }
            UpdateDisplay();
        }
    }

    /// <summary>
    /// Метод для установки значения числа e
    /// </summary>
    private void SetEulerNumber()
    {
        currentValue = Math.E.ToString(CultureInfo.CurrentCulture);
        UpdateDisplay();
        UpdateOperationHistory();
    }
    
    /// <summary>
    /// Метод для возведения в квадрат
    /// </summary>
    private void Squaring()
    {
        if (!string.IsNullOrEmpty(currentValue))
        {
            operationHistory.Append("sqr(" + currentValue + ") =");
            valuesList.Add(currentValue);
            valuesList.Add("^");
            valuesList.Add("2");
            double result = ProcessValues();
            currentValue = result.ToString(CultureInfo.CurrentCulture);
            valuesList.Clear();
            UpdateDisplay();
            UpdateOperationHistory();
        }
    }
    
    /// <summary>
    /// Обновление истории операций
    /// </summary>
    private void UpdateOperationHistory()
    {
        operationHistoryTextBox.Text = operationHistory.ToString();
    }
    
    /// <summary>
    /// Обработка введенных значений
    /// </summary>
    private double ProcessValues()
    {
        if (valuesList.Count == 0)
        {
            return 0; // Если список пуст, возвращаем 0
        }
        
        double result = double.Parse(valuesList[0], CultureInfo.CurrentCulture);

        for (int i = 1; i < valuesList.Count; i += 2)
        {
            if (i + 1 >= valuesList.Count)
            {
                throw new InvalidOperationException("Некорректный формат ввода");
            }

            string operation = valuesList[i];
            double operand = double.Parse(valuesList[i + 1], CultureInfo.CurrentCulture);

            result = PerformOperations(result, operand, operation);
        }
        return result;
    }
    
    /// <summary>
    /// Вычисление операций
    /// </summary>
    private double PerformOperations(double operand1, double operand2, string operation)
    {
        switch (operation)
        {
            case "+": return operand1 + operand2;
            case "-": return operand1 - operand2;
            case "*": return operand1 * operand2;
            case "/":
                if (Math.Abs(operand2) < epsilon)
                {
                    displayTextBox.Text = "Ошибка";
                    return double.NaN;
                }
                return operand1 / operand2;
            case "^": return Math.Pow(operand1, operand2);
            default: throw new InvalidOperationException("Некорректный тип операции");
        }
    }
    
    /// <summary>
    /// Обновление окна
    /// </summary>
    private void UpdateDisplay()
    {
        if (string.IsNullOrEmpty(currentValue))
        {
            displayTextBox.Text = "0"; 
        }
        else
        {
            // Проверяем, является ли текущее значение числом
            if (double.TryParse(currentValue, NumberStyles.Any, CultureInfo.CurrentCulture, out _))
            {
                displayTextBox.Text = currentValue;
            }
            else if (currentValue == "Ошибка")
            {
                displayTextBox.Text = "Ошибка";
            }
            else if (currentValue == "Результат не определен")
            {
                displayTextBox.Text = "Результат не определен";
            }
            else if (currentValue == "Неверный ввод")
            {
                displayTextBox.Text = "Неверный ввод";
            }
        }
    }

    /// <summary>
    /// Проверка на числовое значение
    /// </summary>
    private bool IsNumeric(string value)
    {
        return double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out _);
    }
    
    /// <summary>
    /// Переключение режима (обычный/расширенный)
    /// </summary>
    private void SwitchMode_Click(object sender, RoutedEventArgs e)
    {
        isExtendedMode = !isExtendedMode;
    
        if (isExtendedMode)
        {
            extendedModePanel.Visibility = Visibility.Visible;
            switchModeButton.Content = "Обычн.";
        }
        else
        {
            extendedModePanel.Visibility = Visibility.Collapsed;
            switchModeButton.Content = "Расш.";
        }
    }
    
    /// <summary>
    /// Обработчик кнопок расширенного режима
    /// </summary>
    private void ExtendedButton_Click(object sender, RoutedEventArgs e)
    {
        Button button = (Button)sender;
        string buttonText = button.Content.ToString();
        
        switch (buttonText)
        {
            case "x^y":
                HandlePowerOperation("^");
                break;
            case "ln(x)":
                HandleTrigonometricFunction("ln");
                break;
            case "sin(x)":
                HandleTrigonometricFunction("sin");
                break;
            case "cos(x)":
                HandleTrigonometricFunction("cos");
                break;
            case "tg(x)":
                HandleTrigonometricFunction("tg");
                break;
            case "ctg(x)":
                HandleTrigonometricFunction("ctg");
                break;
            case "√x":
                HandleSquareRootOperation();
                break;
        }
    }

    /// <summary>
    /// Метод для обработки операции возведения в степень
    /// </summary>
    private void HandlePowerOperation(string operation)
    {
        if (!string.IsNullOrEmpty(currentValue))
        {
            operationHistory.Append(currentValue + " " + operation);
            valuesList.Add(currentValue);
            valuesList.Add(operation);
            currentValue = string.Empty;
            UpdateDisplay();
            UpdateOperationHistory();
        }
    }

    /// <summary>
    /// Метод для обработки тригонометрических функций
    /// </summary>
    private void HandleTrigonometricFunction(string functionName)
    {
        if (!string.IsNullOrEmpty(currentValue))
        {
            operationHistory.Append(functionName + "(" + currentValue + ") =");
            double value;
        
            if (!double.TryParse(currentValue, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
            {
                displayTextBox.Text = "Неверный ввод";
                currentValue = string.Empty;
                operationHistory.Clear();
                UpdateOperationHistory();
                return;
            }

            double result;

            try
            {
                switch (functionName)
                {
                    case "ln":
                        result = ComputeLn(value, epsilon);
                        break;
                    case "sin":
                        result = ComputeSin(value, epsilon);
                        break;
                    case "cos":
                        result = ComputeCos(value, epsilon);
                        break;
                    case "tg":
                        result = ComputeTg(value, epsilon);
                        break;
                    case "ctg":
                        result = ComputeCtg(value, epsilon);
                        break;
                    default:
                        throw new ArgumentException("Неподдерживаемая функция тригонометрии");
                }

                if (double.IsNaN(result))
                {
                    displayTextBox.Text = "Неверный ввод";
                    currentValue = string.Empty;
                    operationHistory.Clear();
                    UpdateOperationHistory();
                    return;
                }

                currentValue = result.ToString(CultureInfo.CurrentCulture);
            }
            catch
            {
                displayTextBox.Text = "Неверный ввод";
                currentValue = string.Empty;
                operationHistory.Clear();
                UpdateOperationHistory();
                return;
            }
        
            UpdateDisplay();
            UpdateOperationHistory();
        }
    }

    /// <summary>
    /// Метод для обработки операции квадратного корня
    /// </summary>
    private void HandleSquareRootOperation()
    {
        if (!string.IsNullOrEmpty(currentValue))
        {
            operationHistory.Append("√" + currentValue + " =");
            double value = double.Parse(currentValue, CultureInfo.CurrentCulture);
            double result = ComputeSquareRoot(value);
            currentValue = result.ToString(CultureInfo.CurrentCulture);
            UpdateDisplay();
            UpdateOperationHistory();
        }
    }

    /// <summary>
    /// Переключение режима для тригонометрических функций (градусы/радианы)
    /// </summary>
    private void TrigonometricModeButton_Click(object sender, RoutedEventArgs e)
    {
        isDegreeMode = !isDegreeMode; 

        if (isDegreeMode)
        {
            trigonometricModeButton.Content = "Градусы";
        }
        else
        {
            trigonometricModeButton.Content = "Радианы";
        }
    }

    /// <summary>
    /// Конвертация угла в радианы
    /// </summary>
    private double ConvertToRadians(double angle)
    {
        if (isDegreeMode)
        {
            return angle * Math.PI / 180.0; // Перевод из градусов в радианы
        }
        else
        {
            return angle; 
        }
    }
    
    /// <summary>
    /// Вычисление значения косинуса
    /// </summary>
    private double ComputeCos(double x, double epsilon)
    {
        x = ConvertToRadians(x); // Конвертация угла в радианы

        // Приведение угла к отрезку [0, 2Pi]
        while (x < 0)
        {
            x += 2 * Math.PI;
        }
        while (x >= 2 * Math.PI)
        {
            x -= 2 * Math.PI;
        }

        // Приближенное значение cos(x)
        double cosValue = 1; // Первый член ряда
        double term = 1; // Текущий член ряда
        int i = 2; // Степень

        while (Math.Abs(term) >= epsilon)
        {
            term *= -x * x / ((i - 1) * i); // Следующий член ряда
            cosValue += term; // Добавление члена к результату
            i += 2; // Увеличение степени
        }
        return cosValue;
    }

    /// <summary>
    /// Вычисление значения синуса
    /// </summary>
    private double ComputeSin(double x, double epsilon)
    {
        x = ConvertToRadians(x); 
        
        // Приведение угла к отрезку [-Pi, Pi]
        while (x < -Math.PI)
        {
            x += 2 * Math.PI;
        }
        while (x >= Math.PI)
        {
            x -= 2 * Math.PI;
        }

        // Приближенное значение sin(x)
        double sinValue = 0;
        double term = x; // Первый член ряда

        int i = 1;
        while (Math.Abs(term) >= epsilon)
        {
            sinValue += term;
            term *= -1 * x * x / ((2 * i) * (2 * i + 1)); // Следующий член ряда
            i++;
        }
        return sinValue;
    }

    /// <summary>
    /// Вычисление значения тангенса
    /// </summary>
    private double ComputeTg(double x, double epsilon)
    {
        double sinX = ComputeSin(x, epsilon);
        double cosX = ComputeCos(x, epsilon);

        // Проверка деления на ноль
        if (Math.Abs(cosX) < epsilon)
        {
            throw new ArgumentException("Тангенс угла не определен (деление на ноль)");
        }
        
        double tanValue = sinX / cosX;
        return tanValue;
    }

    /// <summary>
    /// Вычисление значения котангенса
    /// </summary>
    private double ComputeCtg(double x, double epsilon)
    {
        double sinX = ComputeSin(x, epsilon);
        double cosX = ComputeCos(x, epsilon);

        // Проверка деления на ноль
        if (Math.Abs(sinX) < epsilon)
        {
            throw new ArgumentException("Котангенс угла не определен (деление на ноль)");
        }

        double tanValue = cosX / sinX;
        return tanValue;
    }

    /// <summary>
    /// Вычисление натурального логарифма
    /// </summary>
    private double ComputeLn(double x, double epsilon)
    {
        if (x <= 0)
        {
            return double.NaN;
        }

        // Приближенное значение ln(x)
        double ln = 0;
        double term = (x - 1) / x; // Первый член ряда

        int i = 1;
        while (Math.Abs(term) >= epsilon)
        {
            ln += term;
            term *= ((x - 1) / x) * i / (i + 1);
            i++;
        }
        return ln;
    }


    /// <summary>
    /// Вычисление квадратного корня по формуле Герона
    /// </summary>
    private double ComputeSquareRoot(double value)
    {
        if (value < 0)
        {
            displayTextBox.Text = "Ошибка";
            return double.NaN;
        }
        
        if (value == 0)
        {
            return 0;
        }
        
        double x = value;
        while (true)
        {
            double nextX = 0.5 * (x + value / x); // Итерационная формула Герона
            if (Math.Abs(nextX - x) < epsilon) // Проверка на достижение необходимой точности
            {
                return nextX;
            }
            x = nextX;
        }
    }
    
    /// <summary>
    /// Обработчик события PreviewKeyDown для комбинированного ввода
    /// </summary>
    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Проверяем, является ли нажатая клавиша цифрой
        if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
        {
            // Получаем цифру из клавиши
            int digit = (e.Key >= Key.D0 && e.Key <= Key.D9) ? e.Key - Key.D0 : e.Key - Key.NumPad0;
            ProcessDigit(digit.ToString());
        }
        // Проверяем, является ли нажатая клавиша клавишей с точкой (для десятичной части)
        else if (e.Key == Key.OemPeriod || e.Key == Key.Decimal)
        {
            ProcessDigit(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
        }
        else
        {
            e.Handled = true;
        }
    }
    
    /// <summary>
    /// Обработка ввода цифр
    /// </summary>
    private void ProcessDigit(string digit)
    {
        if (currentValue == "Ошибка" || currentValue == "Неверный ввод")
        {
            currentValue = digit;
            operationHistory.Clear();
            UpdateOperationHistory();
        }
        else if (currentValue.Contains("не число"))
        {
            if (digit == CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
            {
                currentValue += digit; 
            }
            else
            {
                currentValue = digit; 
            }
        }
        else
        {
            currentValue += digit;
        }
        UpdateDisplay();
    }
    
    /// <summary>
    /// Обработка нажатия кнопки Backspace
    /// </summary>
    private void BackspaceButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(currentValue))
        {
            if (operationHistory.Length > 0)
            {
                operationHistory.Clear();
                UpdateOperationHistory();
            }
            else
            {
                currentValue = currentValue.Substring(0, currentValue.Length - 1);
                UpdateDisplay();
            }
        }
    }
}
