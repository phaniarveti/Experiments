﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using Umbraco.Core.Configuration;
using System.Web.Security;

namespace Umbraco.Core
{

    ///<summary>
    /// String extension methods
    ///</summary>
    public static class StringExtensions
    {
		/// <summary>
		/// Encrypt the string using the MachineKey in medium trust
		/// </summary>
		/// <param name="value">The string value to be encrypted.</param>
		/// <returns>The encrypted string.</returns>
		public static string EncryptWithMachineKey(this string value)
        {
			if (value == null)
				return null;

			string valueToEncrypt = value;
			List<string> parts = new List<string>();

			const int EncrpytBlockSize = 500;

			while (valueToEncrypt.Length > EncrpytBlockSize)
			{
				parts.Add(valueToEncrypt.Substring(0, EncrpytBlockSize));
				valueToEncrypt = valueToEncrypt.Remove(0, EncrpytBlockSize);
			}

			if (valueToEncrypt.Length > 0)
			{
				parts.Add(valueToEncrypt);
			}

			StringBuilder encrpytedValue = new StringBuilder();

			foreach (var part in parts)
			{
				var encrpytedBlock = FormsAuthentication.Encrypt(new FormsAuthenticationTicket(0, string.Empty, DateTime.Now, DateTime.MaxValue, false, part));
				encrpytedValue.AppendLine(encrpytedBlock);
			}

			return encrpytedValue.ToString().TrimEnd();
        }
		/// <summary>
		/// Decrypt the encrypted string using the Machine key in medium trust
		/// </summary>
		/// <param name="value">The string value to be decrypted</param>
		/// <returns>The decrypted string.</returns>
		public static string DecryptWithMachineKey(this string value)
        {
			if (value == null)
				return null;

			string[] parts = value.Split('\n');

			StringBuilder decryptedValue = new StringBuilder();

			foreach (var part in parts)
			{
				decryptedValue.Append(FormsAuthentication.Decrypt(part.TrimEnd()).UserData);
			}

			return decryptedValue.ToString();
        }
        //this is from SqlMetal and just makes it a bit of fun to allow pluralisation
        public static string MakePluralName(this string name)
        {
            if ((name.EndsWith("x", StringComparison.OrdinalIgnoreCase) || name.EndsWith("ch", StringComparison.OrdinalIgnoreCase)) || (name.EndsWith("ss", StringComparison.OrdinalIgnoreCase) || name.EndsWith("sh", StringComparison.OrdinalIgnoreCase)))
            {
                name = name + "es";
                return name;
            }
            if ((name.EndsWith("y", StringComparison.OrdinalIgnoreCase) && (name.Length > 1)) && !IsVowel(name[name.Length - 2]))
            {
                name = name.Remove(name.Length - 1, 1);
                name = name + "ies";
                return name;
            }
            if (!name.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                name = name + "s";
            }
            return name;
        }

        public static bool IsVowel(this char c)
        {
            switch (c)
            {
                case 'O':
                case 'U':
                case 'Y':
                case 'A':
                case 'E':
                case 'I':
                case 'o':
                case 'u':
                case 'y':
                case 'a':
                case 'e':
                case 'i':
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Trims the specified value from a string; accepts a string input whereas the in-built implementation only accepts char or char[].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="forRemoving">For removing.</param>
        /// <returns></returns>
        public static string Trim(this string value, string forRemoving)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.TrimEnd(forRemoving).TrimStart(forRemoving);
        }

        public static string EncodeJsString(this string s)
        {
            var sb = new StringBuilder();
            foreach (var c in s)
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        int i = (int)c;
                        if (i < 32 || i > 127)
                        {
                            sb.AppendFormat("\\u{0:X04}", i);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            return sb.ToString();
        }

        public static string TrimEnd(this string value, string forRemoving)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (string.IsNullOrEmpty(forRemoving)) return value;

            while (value.EndsWith(forRemoving, StringComparison.InvariantCultureIgnoreCase))
            {
                value = value.Remove(value.LastIndexOf(forRemoving, StringComparison.InvariantCultureIgnoreCase));
            }
            return value;
        }

        public static string TrimStart(this string value, string forRemoving)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (string.IsNullOrEmpty(forRemoving)) return value;

            while (value.StartsWith(forRemoving, StringComparison.InvariantCultureIgnoreCase))
            {
                value = value.Substring(forRemoving.Length);
            }
            return value;
        }

        public static string EnsureStartsWith(this string input, string toStartWith)
        {
            if (input.StartsWith(toStartWith)) return input;
            return toStartWith + input.TrimStart(toStartWith.ToArray()); // Ensure each char is removed first from input, e.g. ~/ plus /Path will equal ~/Path not ~//Path
        }

        public static string EnsureStartsWith(this string input, char value)
        {
			return input.StartsWith(value.ToString(CultureInfo.InvariantCulture)) ? input : value + input;
        }

        public static string EnsureEndsWith(this string input, char value)
        {
			return input.EndsWith(value.ToString(CultureInfo.InvariantCulture)) ? input : input + value;
        }

        public static bool IsLowerCase(this char ch)
        {
            return ch.ToString(CultureInfo.InvariantCulture) == ch.ToString(CultureInfo.InvariantCulture).ToLowerInvariant();
        }

        public static bool IsUpperCase(this char ch)
        {
			return ch.ToString(CultureInfo.InvariantCulture) == ch.ToString(CultureInfo.InvariantCulture).ToUpperInvariant();
        }

        /// <summary>Is null or white space.</summary>
        /// <param name="str">The str.</param>
        /// <returns>The is null or white space.</returns>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return (str == null) || (str.Trim().Length == 0);
        }

        public static string IfNullOrWhiteSpace(this string str, string defaultValue)
        {
            return str.IsNullOrWhiteSpace() ? defaultValue : str;
        }

        /// <summary>The to delimited list.</summary>
        /// <param name="list">The list.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <returns>the list</returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "By design")]
        public static IList<string> ToDelimitedList(this string list, string delimiter = ",")
        {
            var delimiters = new[] { delimiter };
            return !list.IsNullOrWhiteSpace()
                       ? list.Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
                             .Select(i => i.Trim())
                             .ToList()
                       : new List<string>();
        }

        /// <summary>enum try parse.</summary>
        /// <param name="strType">The str type.</param>
        /// <param name="ignoreCase">The ignore case.</param>
        /// <param name="result">The result.</param>
        /// <typeparam name="T">The type</typeparam>
        /// <returns>The enum try parse.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "By Design")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "By Design")]
        public static bool EnumTryParse<T>(this string strType, bool ignoreCase, out T result)
        {
            try
            {
                result = (T)Enum.Parse(typeof(T), strType, ignoreCase);
                return true;
            }
            catch
            {
                result = default(T);
                return false;
            }
        }

        /// <summary>
        /// Parse string to Enum
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <param name="strType">The string to parse</param>
        /// <param name="ignoreCase">The ignore case</param>
        /// <returns>The parsed enum</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "By Design")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "By Design")]
        public static T EnumParse<T>(this string strType, bool ignoreCase)
        {
            return (T)Enum.Parse(typeof(T), strType, ignoreCase);
        }

        /// <summary>
        /// Strips all html from a string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>Returns the string without any html tags.</returns>
        public static string StripHtml(this string text)
        {
            const string pattern = @"<(.|\n)*?>";
            return Regex.Replace(text, pattern, String.Empty);
        }

        /// <summary>
        /// Converts string to a URL alias.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="charReplacements">The char replacements.</param>
        /// <param name="replaceDoubleDashes">if set to <c>true</c> replace double dashes.</param>
        /// <param name="stripNonAscii">if set to <c>true</c> strip non ASCII.</param>
        /// <param name="urlEncode">if set to <c>true</c> URL encode.</param>
        /// <returns></returns>
        /// <remarks>
        /// This ensures that ONLY ascii chars are allowed and of those ascii chars, only digits and lowercase chars, all
        /// punctuation, etc... are stripped out, however this method allows you to pass in string's to replace with the
        /// specified replacement character before the string is converted to ascii and it has invalid characters stripped out.
        /// This allows you to replace strings like &amp; , etc.. with your replacement character before the automatic
        /// reduction.
        /// </remarks>
        public static string ToUrlAlias(this string value, IDictionary<string, string> charReplacements, bool replaceDoubleDashes, bool stripNonAscii, bool urlEncode)
        {
            //first to lower case
            value = value.ToLowerInvariant();

            //then replacement chars
            value = charReplacements.Aggregate(value, (current, kvp) => current.Replace(kvp.Key, kvp.Value));

            //then convert to only ascii, this will remove the rest of any invalid chars
            if (stripNonAscii)
            {
                value = Encoding.ASCII.GetString(
                    Encoding.Convert(
                        Encoding.UTF8,
                        Encoding.GetEncoding(
                            Encoding.ASCII.EncodingName,
                            new EncoderReplacementFallback(String.Empty),
                            new DecoderExceptionFallback()),
                        Encoding.UTF8.GetBytes(value)));

                //remove all characters that do not fall into the following categories (apart from the replacement val)
                var validCodeRanges =
                    //digits
                    Enumerable.Range(48, 10).Concat(
                    //lowercase chars
                        Enumerable.Range(97, 26));

                var sb = new StringBuilder();
                foreach (var c in value.Where(c => charReplacements.Values.Contains(c.ToString(CultureInfo.InvariantCulture)) || validCodeRanges.Contains(c)))
                {
                    sb.Append(c);
                }

                value = sb.ToString();
            }

            //trim dashes from end
            value = value.Trim('-', '_');

            //replace double occurances of - or _
            value = replaceDoubleDashes ? Regex.Replace(value, @"([-_]){2,}", "$1", RegexOptions.Compiled) : value;

            //url encode result
            return urlEncode ? HttpUtility.UrlEncode(value) : value;
        }

        /// <summary>
        /// Converts a string for use with an entity alias which is camel case and without invalid characters
        /// </summary>
        /// <param name="phrase">The phrase.</param>
        /// <param name="caseType">By default this is camel case</param>
        /// <param name="removeSpaces">if set to <c>true</c> [remove spaces].</param>
        /// <returns></returns>
        public static string ToUmbracoAlias(this string phrase, StringAliasCaseType caseType = StringAliasCaseType.CamelCase, bool removeSpaces = false)
        {
            if (string.IsNullOrEmpty(phrase)) return string.Empty;

            //convert case first
            var tmp = phrase.ConvertCase(caseType);

            //remove non-alphanumeric chars
            var result = Regex.Replace(tmp, @"[^a-zA-Z0-9\s\.-]+", "", RegexOptions.Compiled);

            if (removeSpaces)
                result = result.Replace(" ", "");

            return result;
        }

        /// <summary>
        /// Splits a Pascal cased string into a phrase seperated by spaces.
        /// </summary>
        /// <param name="phrase">String to split</param>
        /// <returns></returns>
        public static string SplitPascalCasing(this string phrase)
        {
            string result = Regex.Replace(phrase, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
            return result;
        }

        /// <summary>
        /// Converts the phrase to specified convention.
        /// </summary>
        /// <param name="phrase"></param>
        /// <param name="cases">The cases.</param>
        /// <returns>string</returns>
        public static string ConvertCase(this string phrase, StringAliasCaseType cases)
        {
            var splittedPhrase = Regex.Split(phrase, @"[^a-zA-Z0-9\']", RegexOptions.Compiled);

            if (cases == StringAliasCaseType.Unchanged)
                return string.Join("", splittedPhrase);

            //var splittedPhrase = phrase.Split(' ', '-', '.');
            var sb = new StringBuilder();

            foreach (var splittedPhraseChars in splittedPhrase.Select(s => s.ToCharArray()))
            {
                if (splittedPhraseChars.Length > 0)
                {
                    splittedPhraseChars[0] = ((new String(splittedPhraseChars[0], 1)).ToUpperInvariant().ToCharArray())[0];
                }
                sb.Append(new String(splittedPhraseChars));
            }

            var result = sb.ToString();

            if (cases == StringAliasCaseType.CamelCase)
            {
                if (result.Length > 1)
                {
                    var pattern = new Regex("^([A-Z]*)([A-Z].*)$", RegexOptions.Singleline | RegexOptions.Compiled);
                    var match = pattern.Match(result);
                    if (match.Success)
                    {
                        result = match.Groups[1].Value.ToLowerInvariant() + match.Groups[2].Value;

                        return result.Substring(0, 1).ToLowerInvariant() + result.Substring(1);
                    }

                    return result;
                }

                return result.ToLowerInvariant();
            }

            return result;
        }

        /// <summary>
        /// Encodes as GUID.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Guid EncodeAsGuid(this string input)
        {
            if (string.IsNullOrWhiteSpace(input)) throw new ArgumentNullException("input");

            var convertToHex = input.ConvertToHex();
            var hexLength = convertToHex.Length < 32 ? convertToHex.Length : 32;
            var hex = convertToHex.Substring(0, hexLength).PadLeft(32, '0');
            var output = Guid.Empty;
            return Guid.TryParse(hex, out output) ? output : Guid.Empty;
        }

        /// <summary>
        /// Converts to hex.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string ConvertToHex(this string input)
        {
            if (String.IsNullOrEmpty(input)) return String.Empty;

            var sb = new StringBuilder(input.Length);
            foreach (char c in input)
            {
                int tmp = c;
                sb.AppendFormat("{0:x2}", Convert.ToUInt32(c));
            }
            return sb.ToString();
        }

        ///<summary>
        /// Encodes a string to a safe URL base64 string
        ///</summary>
        ///<param name="input"></param>
        ///<returns></returns>
        public static string ToUrlBase64(this string input)
        {
            if (input == null) throw new ArgumentNullException("input");

            if (String.IsNullOrEmpty(input)) return String.Empty;

            var bytes = Encoding.UTF8.GetBytes(input);
            return UrlTokenEncode(bytes);
            //return Convert.ToBase64String(bytes).Replace(".", "-").Replace("/", "_").Replace("=", ",");
        }

        /// <summary>
        /// Decodes a URL safe base64 string back
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string FromUrlBase64(this string input)
        {
            if (input == null) throw new ArgumentNullException("input");

            //if (input.IsInvalidBase64()) return null;

            try
            {
                //var decodedBytes = Convert.FromBase64String(input.Replace("-", ".").Replace("_", "/").Replace(",", "="));
                byte[] decodedBytes = UrlTokenDecode(input);
                return decodedBytes != null ? Encoding.UTF8.GetString(decodedBytes) : null;
            }
            catch (FormatException ex)
            {
                return null;
            }
        }

        /// <summary>
        /// formats the string with invariant culture
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public static string InvariantFormat(this string format, params object[] args)
        {
            return String.Format(CultureInfo.InvariantCulture, format, args);
        }

        /// <summary>
        /// Compares 2 strings with invariant culture and case ignored
        /// </summary>
        /// <param name="compare">The compare.</param>
        /// <param name="compareTo">The compare to.</param>
        /// <returns></returns>
        public static bool InvariantEquals(this string compare, string compareTo)
        {
            return String.Equals(compare, compareTo, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool InvariantStartsWith(this string compare, string compareTo)
        {
            return compare.StartsWith(compareTo, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool InvariantContains(this string compare, string compareTo)
        {
            return compare.IndexOf(compareTo, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool InvariantContains(this IEnumerable<string> compare, string compareTo)
        {
            return compare.Contains(compareTo, new DelegateEqualityComparer<string>((source, dest) => source.Equals(dest, StringComparison.InvariantCultureIgnoreCase), x => x.GetHashCode()));
        }

        /// <summary>
        /// Determines if the string is a Guid
        /// </summary>
        /// <param name="str"></param>
        /// <param name="withHyphens"></param>
        /// <returns></returns>
        public static bool IsGuid(this string str, bool withHyphens)
        {
            var isGuid = false;

            if (!String.IsNullOrEmpty(str))
            {
                Regex guidRegEx;
                if (withHyphens)
                {
                    guidRegEx = new Regex(@"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$");
                }
                else
                {
                    guidRegEx = new Regex(@"^(\{{0,1}([0-9a-fA-F]){8}([0-9a-fA-F]){4}([0-9a-fA-F]){4}([0-9a-fA-F]){4}([0-9a-fA-F]){12}\}{0,1})$");
                }
                isGuid = guidRegEx.IsMatch(str);
            }

            return isGuid;
        }

        /// <summary>
        /// Tries to parse a string into the supplied type by finding and using the Type's "Parse" method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
        public static T ParseInto<T>(this string val)
        {
            return (T)val.ParseInto(typeof(T));
        }

        /// <summary>
        /// Tries to parse a string into the supplied type by finding and using the Type's "Parse" method
        /// </summary>
        /// <param name="val"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ParseInto(this string val, Type type)
        {
            if (!String.IsNullOrEmpty(val))
            {
                TypeConverter tc = TypeDescriptor.GetConverter(type);
                return tc.ConvertFrom(val);
            }
            return val;
        }

        /// <summary>
        /// Converts the string to MD5
        /// </summary>
        /// <param name="stringToConvert">referrs to itself</param>
        /// <returns>the md5 hashed string</returns>
        public static string ToMd5(this string stringToConvert)
        {
            //create an instance of the MD5CryptoServiceProvider
            var md5Provider = new MD5CryptoServiceProvider();

            //convert our string into byte array
            var byteArray = Encoding.UTF8.GetBytes(stringToConvert);

            //get the hashed values created by our MD5CryptoServiceProvider
            var hashedByteArray = md5Provider.ComputeHash(byteArray);

            //create a StringBuilder object
            var stringBuilder = new StringBuilder();

            //loop to each each byte
            foreach (var b in hashedByteArray)
            {
                //append it to our StringBuilder
                stringBuilder.Append(b.ToString("x2").ToLower());
            }

            //return the hashed value
            return stringBuilder.ToString();
        }


        /// <summary>
        /// Decodes a string that was encoded with UrlTokenEncode
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static byte[] UrlTokenDecode(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            int length = input.Length;
            if (length < 1)
            {
                return new byte[0];
            }
            int num2 = input[length - 1] - '0';
            if ((num2 < 0) || (num2 > 10))
            {
                return null;
            }
            char[] inArray = new char[(length - 1) + num2];
            for (int i = 0; i < (length - 1); i++)
            {
                char ch = input[i];
                switch (ch)
                {
                    case '-':
                        inArray[i] = '+';
                        break;

                    case '_':
                        inArray[i] = '/';
                        break;

                    default:
                        inArray[i] = ch;
                        break;
                }
            }
            for (int j = length - 1; j < inArray.Length; j++)
            {
                inArray[j] = '=';
            }
            return Convert.FromBase64CharArray(inArray, 0, inArray.Length);
        }

        /// <summary>
        /// Encodes a string so that it is 'safe' for URLs, files, etc..
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static string UrlTokenEncode(byte[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (input.Length < 1)
            {
                return String.Empty;
            }
            string str = null;
            int index = 0;
            char[] chArray = null;
            str = Convert.ToBase64String(input);
            if (str == null)
            {
                return null;
            }
            index = str.Length;
            while (index > 0)
            {
                if (str[index - 1] != '=')
                {
                    break;
                }
                index--;
            }
            chArray = new char[index + 1];
            chArray[index] = (char)((0x30 + str.Length) - index);
            for (int i = 0; i < index; i++)
            {
                char ch = str[i];
                switch (ch)
                {
                    case '+':
                        chArray[i] = '-';
                        break;

                    case '/':
                        chArray[i] = '_';
                        break;

                    case '=':
                        chArray[i] = ch;
                        break;

                    default:
                        chArray[i] = ch;
                        break;
                }
            }
            return new string(chArray);
        }

        /// <summary>
        /// Ensures that the folder path endds with a DirectorySeperatorChar
        /// </summary>
        /// <param name="currentFolder"></param>
        /// <returns></returns>
        public static string NormaliseDirectoryPath(this string currentFolder)
        {
            currentFolder = currentFolder
                                .IfNull(x => String.Empty)
                                .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return currentFolder;
        }

        /// <summary>
        /// Truncates the specified text string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="maxLength">Length of the max.</param>
        /// <param name="suffix">The suffix.</param>
        /// <returns></returns>
        public static string Truncate(this string text, int maxLength, string suffix = "...")
        {
            // replaces the truncated string to a ...
            var truncatedString = text;

            if (maxLength <= 0) return truncatedString;
            var strLength = maxLength - suffix.Length;

            if (strLength <= 0) return truncatedString;

            if (text == null || text.Length <= maxLength) return truncatedString;

            truncatedString = text.Substring(0, strLength);
            truncatedString = truncatedString.TrimEnd();
            truncatedString += suffix;

            return truncatedString;
        }

        /// <summary>
        /// Strips carrage returns and line feeds from the specified text.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string StripNewLines(this string input)
        {
            return input.Replace("\r", "").Replace("\n", "");
        }

        public static string OrIfNullOrWhiteSpace(this string input, string alternative)
        {
            return !string.IsNullOrWhiteSpace(input)
                       ? input
                       : alternative;
        }

        public static string FormatUrl(this string url)
        {
            string newUrl = url;
            XmlNode replaceChars = UmbracoSettings.UrlReplaceCharacters;
            foreach (XmlNode n in replaceChars.SelectNodes("char"))
            {
                if (n.Attributes.GetNamedItem("org") != null && n.Attributes.GetNamedItem("org").Value != "")
                    newUrl = newUrl.Replace(n.Attributes.GetNamedItem("org").Value, XmlHelper.GetNodeValue(n));
            }

            // check for double dashes
            if (UmbracoSettings.RemoveDoubleDashesFromUrlReplacing)
            {
                newUrl = Regex.Replace(newUrl, @"[-]{2,}", "-");
            }

            return newUrl;
        }

        /// <summary>
        /// An extention method to ensure that an Alias string doesn't contains any illegal characters
        /// which is defined in a private constant 'ValidCharacters' in this class. 
        /// Conventions over configuration, baby. You can't touch this - MC Hammer!
        /// </summary>
        /// <remarks>
        /// Copied and cleaned up a bit from umbraco.cms.helpers.Casing.
        /// </remarks>
        /// <param name="alias">The alias.</param>
        /// <returns>An alias guaranteed not to contain illegal characters</returns>
        public static string ToSafeAlias(this string alias)
        {
            const string validAliasCharacters = "_-abcdefghijklmnopqrstuvwxyz1234567890";
            const string invalidFirstCharacters = "0123456789";
            var safeString = new StringBuilder();
            int aliasLength = alias.Length;
            for (int i = 0; i < aliasLength; i++)
            {
                string currentChar = alias.Substring(i, 1);
                if (validAliasCharacters.Contains(currentChar.ToLowerInvariant()))
                {
                    // check for camel (if previous character is a space, we'll upper case the current one
                    if (safeString.Length == 0 && invalidFirstCharacters.Contains(currentChar.ToLowerInvariant()))
                    {
                        currentChar = "";
                    }
                    else
                    {
                        if (i < aliasLength - 1 && i > 0 && alias.Substring(i - 1, 1) == " ")
                            currentChar = currentChar.ToUpperInvariant();

                        safeString.Append(currentChar);
                    }
                }
            }

            return safeString.ToString();
        }

        public static string ToSafeAliasWithForcingCheck(this string alias)
        {
            if (UmbracoSettings.ForceSafeAliases)
            {
                return alias.ToSafeAlias();
            }
            
            return alias;
        }
    }
}
