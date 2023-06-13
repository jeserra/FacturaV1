using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;

public class FacturaElectronicaMX
{
    public static string GenerarFactura(Dictionary<string, string> emisor, Dictionary<string, string> receptor, List<Dictionary<string, string>> conceptos, string version)
    {
        bool isVersion2 = version.Equals("2");
        bool isAfter2023 = DateTime.Now.Year > 2023;

        string xmlFactura = $"<Factura version=\"{version}\">";
        string rfcEmisor = String.Empty;
        // Generar el elemento para el emisor
        xmlFactura += "<Emisor>";
        foreach (var kvp in emisor)
        {
            if(kvp.Key == "RFC")
                rfcEmisor = kvp.Value;

            xmlFactura += $"<{kvp.Key}>{kvp.Value}</{kvp.Key}>";
        }
        xmlFactura += "</Emisor>";

        // Generar el elemento para el receptor
        xmlFactura += "<Receptor>";
        foreach (var kvp in receptor)
        {
            xmlFactura += $"<{kvp.Key}>{kvp.Value}</{kvp.Key}>";
        }
        xmlFactura += "</Receptor>";

        // Generar los elementos para los conceptos
        xmlFactura += "<Conceptos>";
        decimal subtotal = 0;
        foreach (var concepto in conceptos)
        {
            xmlFactura += "<Concepto>";
            foreach (var kvp in concepto)
            {
                if (kvp.Key.Equals("Precio") && isVersion2)
                {
                    decimal precio = decimal.Parse(kvp.Value);
                    decimal nuevoPrecio = isAfter2023 ? precio * 1.16m : precio * 1.15m;
                    xmlFactura += $"<{kvp.Key}>{nuevoPrecio}</{kvp.Key}>";
                    subtotal += nuevoPrecio;
                }
                else
                {
                    xmlFactura += $"<{kvp.Key}>{kvp.Value}</{kvp.Key}>";
                    if (kvp.Key.Equals("Precio"))
                    {
                        subtotal += decimal.Parse(kvp.Value);
                    }
                }
            }
            xmlFactura += "</Concepto>";
        }
        xmlFactura += "</Conceptos>";

        decimal impuestos = subtotal * 0.16m;
        decimal total = subtotal + impuestos;

        // Generar los elementos para el subtotal, impuestos y total
        xmlFactura += $"<Subtotal>{subtotal}</Subtotal>";
        xmlFactura += $"<Impuestos>{impuestos}</Impuestos>";
        xmlFactura += $"<Total>{total}</Total>";

        // Calcular el sello
        string sello = CalcularSello(xmlFactura);
        xmlFactura += $"<Sello>{sello}</Sello>";

        xmlFactura += "</Factura>";

        var timbreFiscal = ObtenerTimbreFiscal(rfcEmisor, xmlFactura);

        AgregarTimbreFiscal(xmlFactura, timbreFiscal);
        // Guardar la factura en la base de datos
        GuardarFacturaEnBaseDeDatos(xmlFactura);

        return xmlFactura;
    }

    public static string CalcularSello(string xmlFactura)
    {
        byte[] bytesFactura = Encoding.UTF8.GetBytes(xmlFactura);
        byte[] hash;

        using (SHA256 sha256 = SHA256.Create())
        {
            hash = sha256.ComputeHash(bytesFactura);
        }

        string sello = Convert.ToBase64String(hash);
        return sello;
    }

     public static void GuardarFacturaEnBaseDeDatos(string xmlFactura)
    {
        string connectionString = "Data Source=(local);Initial Catalog=NombreBaseDatos;User ID=Usuario;Password=Contraseña";
        string query = "INSERT INTO Facturas (XMLFactura) VALUES (@xmlFactura)";

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@xmlFactura", xmlFactura);
                command.ExecuteNonQuery();
            }
        }
    }


    public static string ObtenerTimbreFiscal(string rfc, string xmlFactura)
    {
        string servicioSoapUrl = "https://servicio-soap.com/timbrador"; // Reemplaza con la URL correcta del servicio SOAP
        string soapEnvelope = BuildSoapEnvelope(rfc, xmlFactura);

        using (HttpClient httpClient = new HttpClient())
        {
            StringContent content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");

            HttpResponseMessage response = httpClient.PostAsync(servicioSoapUrl, content).Result;
            response.EnsureSuccessStatusCode();

            string responseContent = response.Content.ReadAsStringAsync().Result;
            string timbreFiscal = ExtractTimbreFiscalFromResponse(responseContent);

            return timbreFiscal;
        }
    }

     public static string BuildSoapEnvelope(string rfc, string xmlFactura)
    {
        // Construye el sobre SOAP con los parámetros de entrada
        string soapEnvelope = $@"<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                                    <soap:Body>
                                        <ObtenerTimbreFiscal xmlns=""http://servicio-soap.com"">
                                            <RFC>{rfc}</RFC>
                                            <XMLFactura>{xmlFactura}</XMLFactura>
                                        </ObtenerTimbreFiscal>
                                    </soap:Body>
                                </soap:Envelope>";

        return soapEnvelope;
    }

    public static string ExtractTimbreFiscalFromResponse(string soapResponse)
    {
        // Extrae el campo "timbrefiscal" del XML de respuesta del servicio SOAP
        // Implementa la lógica necesaria para extraer el campo específico de acuerdo a la estructura de la respuesta
        // Ejemplo:
        // XmlDocument xmlDoc = new XmlDocument();
        // xmlDoc.LoadXml(soapResponse);
        // string timbreFiscal = xmlDoc.SelectSingleNode("//*[local-name()='TimbreFiscal']").InnerXml;

        // Se omite la implementación específica en este ejemplo
        string timbreFiscal = "<TimbreFiscal>...</TimbreFiscal>";

        return timbreFiscal;
    }

      public static string AgregarTimbreFiscal(string xmlFactura, string timbreFiscal)
    {
        // Agrega el campo "timbrefiscal" al XML de la factura original
        // Implementa la lógica necesaria para agregar el campo en la posición y formato correctos
        // Ejemplo:
        // XmlDocument xmlDoc = new XmlDocument();
        // xmlDoc.LoadXml(xmlFactura);
        // XmlElement timbreFiscalElement = xmlDoc.CreateElement("TimbreFiscal");
        // timbreFiscalElement.InnerXml = timbreFiscal;
        // xmlDoc.DocumentElement.AppendChild(timbreFiscalElement);
        // string facturaConTimbreFiscal = xmlDoc.OuterXml;

        // Se omite la implementación específica en este ejemplo
        string facturaConTimbreFiscal = xmlFactura + timbreFiscal;

        return facturaConTimbreFiscal;
    }
    // Ejemplo de uso
    
}
