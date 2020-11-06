using System;
using System.IO;
using System.Net;
using System.Text.Json;

namespace Simple_Requests
{
    class UI
    {
        private static readonly string baseUrl = "http://dummy.restapiexample.com/api/v1";

        public static void Init()
        {
            Console.WriteLine();
            string input;
            Console.WriteLine("Type in a command" +
                "\n add: add a new employee menu" +
                "\n getall: gets a list of employees" + //the API doesnt return list of all employees, assume it is paginated
                "\n get: gets a show employee menu and create instance of an Employee class" +
                "\n put: gets change data menu for employee" +
                "\n delete: gets delete menu for employee" +
                "\n q: exit from the program");
            Console.WriteLine("---");
            input = Console.ReadLine();
            switch (input)
            {
                case ("add"):
                    CreateNewEmployee();
                    break;
                case ("get"):
                    GetEmployeeByIdAndMakeObject();
                    break;
                case ("put"):
                    ChangeEmployeeData();
                    break;
                case ("delete"):
                    DeleteEmployeeById();
                    break;
                case ("q"):
                    break;
                default:
                    Init();
                    break;
            }
        }

        private static void DeleteEmployeeById()  //API only works with JSONS that it has
        {
            Console.WriteLine("" +
                "\nYou will be prompted to send DELETE request" +
                "\nType in an ID to delete" +
                "\n---");

            int idInput = int.Parse(Console.ReadLine());
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{baseUrl}/delete/{idInput}");
            request.Method = "DELETE";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                var result = stream.ReadToEnd();
                Console.WriteLine(result);
            }
            response.Close();
            Init();
        }

        private static void ChangeEmployeeData()
        {
            Console.WriteLine("" +
                "\nYou will be prompted to send PUT request" +
                "\nType in an ID to change data" +
                "\n---");
            int idInput = int.Parse(Console.ReadLine());
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{baseUrl}/update/{idInput}");
            request.Method = "PUT";
            request.ContentType = "application/json";

            ShowEmployeeGetRequest(idInput); // Create get request to display found employee (if any) - for readability purposes

            Console.WriteLine("Type in new values for this person" +
                "\n");
            Console.WriteLine("Name:");
            string name = Console.ReadLine();
            Console.WriteLine("Salary:");
            int salary = int.Parse(Console.ReadLine());
            Console.WriteLine("Age:");
            int age = int.Parse(Console.ReadLine());

            Employee employee = new Employee(name, salary, age);
            var employeeSerialised = JsonSerializer.Serialize(employee);
            JsonDocument employeeJson = JsonDocument.Parse(employeeSerialised);
            Console.WriteLine(employeeJson);

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(employeeSerialised);
            }

            var response = (HttpWebResponse)request.GetResponse();

            Console.WriteLine("---");
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                Console.WriteLine(streamReader.ReadToEnd());
            }
            response.Close();
            Init();
        }

        //Creates a new employee object, serializes it and sends to the API doesn't check for input type errors
        private static void CreateNewEmployee()
        {
            Console.WriteLine("" +
                "\nYou will be promted to write dummy employee information" +
                "\nTyped in information will be submitted as JSON serialized POST HTTP request" +
                "\n---");

            Console.Write("Type in person's name: ");
            string name = Console.ReadLine();
            Console.Write("Type in person's salary: ");
            int salary = int.Parse(Console.ReadLine());
            Console.Write("Type in person's age: ");
            int age = int.Parse(Console.ReadLine());

            string dataString = "";
            try
            {
                Employee employee = new Employee(name, salary, age);
                dataString = JsonSerializer.Serialize(employee);
            }
            catch (Exception)
            {
                Console.WriteLine("Please type in appropriate data");
                Init();
            }

            Console.WriteLine("Data to be posted:");
            Console.WriteLine(dataString);

            SubmitNewJSON(dataString, $"{baseUrl}/create");
        }

        //Gets an employee by Id, if status is "success" meaning I got data back, then it serializes JSON and casts to Employee object
        private static void GetEmployeeByIdAndMakeObject()
        {
            Console.WriteLine("Please provide a numeric id:");
            int idInput = int.Parse(Console.ReadLine());

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{baseUrl}/employee/{idInput}");
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                var stringifiedData = stream.ReadToEnd();

                using (JsonDocument dataInJson = JsonDocument.Parse(stringifiedData))
                {

                    JsonElement rootLoc = dataInJson.RootElement;
                    JsonElement dataElement = rootLoc.GetProperty("data");
                    if (!rootLoc.TryGetProperty("data", out rootLoc))
                    {
                        Console.WriteLine("Unable to find Employee with given ID. Try another one");
                        response.Close();
                        GetEmployeeByIdAndMakeObject();
                    }
                    else
                    {
                        Employee employee;
                        try
                        {
                            employee = new Employee(
                                dataElement.GetProperty("employee_name").GetString(), //name
                                dataElement.GetProperty("employee_salary").GetInt32(), //salary
                                dataElement.GetProperty("employee_age").GetInt32()  //age
                                );
                            Console.WriteLine();
                            Console.WriteLine("Printing newly created employee object");
                            Console.WriteLine(employee);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Unable to create employee, may be searched employee with the given ID doesn't exist " +
                                "or provided data is incomplete");
                            Console.WriteLine(e.Message);
                        }

                        //... do something else to the object
                        Console.WriteLine("---");
                        response.Close();
                        Init();
                    }
                }
            }
        }

        private static void ShowEmployeeGetRequest(int id)
        {
            HttpWebRequest requestGet = (HttpWebRequest)WebRequest.Create($"{baseUrl}/employee/{id}");
            requestGet.Method = "GET";
            HttpWebResponse responseGet = (HttpWebResponse)requestGet.GetResponse();
            using (var reader = new StreamReader(responseGet.GetResponseStream()))
            {
                Console.WriteLine(reader.ReadToEnd());
            }
        }

        //Can be used to send different kinds of POST raw JSON content type requests
        private static void SubmitNewJSON(string stringifiedData, string postUri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(postUri);
            request.Method = "POST";
            request.ContentType = "application/json";

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(stringifiedData);
            }

            var response = (HttpWebResponse)request.GetResponse();

            Console.WriteLine("---");
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                Console.WriteLine(streamReader.ReadToEnd());
            }
            response.Close();
            Init();
        }
    }
}
