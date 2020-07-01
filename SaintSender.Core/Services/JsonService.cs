using SaintSender.Core.Entities;
using System;
using System.IO;
using System.Text.Json;

namespace SaintSender.Core.Services
{
    public class JsonService
    {
        //todo create unit test
        /// <summary>
        /// Serialize the user object in a json file. Write it only
        /// </summary>
        /// <param name="user"> UserData object with Email and Password
        /// properties</param>
        public static void SerializeToJson(UserData user)
        {
            if (user.Email != null && user.Password != null)
            {
                string jsonText = JsonSerializer.Serialize(user);
                WriteJsonInDirectory(Environment.CurrentDirectory + @"\credentials.json", jsonText);
            }
        }

        //todo create unit test
        /// <summary>
        /// Create and write json file in directory
        /// </summary>
        /// <param name="path">where to write the file</param>
        /// <param name="jsonText">json text after serialization</param>
        private static void WriteJsonInDirectory(string path, string jsonText)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                using (StreamWriter streamWriter = new StreamWriter(path, true))
                {
                    streamWriter.WriteLine(jsonText);
                    streamWriter.Close();
                }
            }
            else
            {
                using (StreamWriter streamWriter = new StreamWriter(path, true))
                {
                    streamWriter.WriteLine(jsonText);
                    streamWriter.Close();
                }
            }
        }

        //todo create unit test
        /// <summary>
        /// Create UserData object from json file
        /// </summary>
        /// <param name="path"> file path </param>
        /// <returns>UserData object </returns>
        public static UserData DeserializeJsonFile(string path)
        {
            UserData userData = null;

            try
            {
                string jsonString = File.ReadAllText(path);
                userData = JsonSerializer.Deserialize<UserData>(jsonString);
                return userData;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return userData;
        }
    }
}