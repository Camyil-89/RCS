using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RCS.Service.UI
{
	public class MessageBoxHelper
	{

		/// <summary>
		/// Окно сообщения содержит символ, состоящий из восклицательного знака в треугольнике
		///     с желтым фоном.
		/// </summary>
		/// <param name="text"></param>
		public static void WarningShow(string text)
		{
			MessageBox.Show(text, "Российски стандарт сертификации Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
		}
		/// <summary>
		/// Окно сообщения содержит символ, состоящий из белого X в кружке с красным фоном.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="er"></param>
		public static void ErrorShow(string text, Exception er)
		{
			MessageBox.Show($"{text}\n\n{er}", "Российски стандарт сертификации Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
		}
		/// <summary>
		/// Окно сообщения содержит символ, состоящий из белого X в кружке с красным фоном.
		/// </summary>
		/// <param name="text"></param>
		public static void ErrorShow(string text)
		{
			MessageBox.Show(text, "Российски стандарт сертификации Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
		}
		/// <summary>
		/// Окно сообщения содержит символ, состоящий из строчной буквы в кружке.
		/// </summary>
		/// <param name="text"></param>
		public static void InfoShow(string text)
		{
			MessageBox.Show(text, "Российски стандарт сертификации Информация", MessageBoxButton.OK, MessageBoxImage.Information);
		}
		/// <summary>
		/// Окно сообщения содержит символ, состоящий из строчной буквы в кружке.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="title"></param>
		public static void InfoShow(string text, string title)
		{
			MessageBox.Show(text, $"Российски стандарт сертификации {title}", MessageBoxButton.OK, MessageBoxImage.Information);
		}
		/// <summary>
		/// Окно сообщения содержит символ, состоящий из восклицательного знака в треугольнике
		///     с желтым фоном.
		/// </summary>
		/// <param name="text"></param>
		public static void ExclamationShow(string text)
		{
			MessageBox.Show(text, "Российски стандарт сертификации Информация", MessageBoxButton.OK, MessageBoxImage.Exclamation);
		}
		/// <summary>
		/// Окно сообщения содержит символ, состоящий из вопросительного знака в кружке.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static MessageBoxResult QuestionShow(string text)
		{
			return MessageBox.Show(text, "Российски стандарт сертификации Вопрос", MessageBoxButton.YesNo, MessageBoxImage.Question);
		}
		/// <summary>
		/// Окно сообщения содержит символ, состоящий из вопросительного знака в кружке.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static MessageBoxResult QuestionShowTopMost(string text)
		{
			return MessageBox.Show(text, "Российски стандарт сертификации Вопрос", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.None, MessageBoxOptions.DefaultDesktopOnly);
		}

	}
}
