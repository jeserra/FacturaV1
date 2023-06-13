 Dictionary<string, string> emisor = new Dictionary<string, string>()
        {
            { "RFC", "EMISOR123" },
            { "Razon Social", "Empresa Emisora" },
            { "Domicilio", "Calle 123, Ciudad" }
        };

        Dictionary<string, string> receptor = new Dictionary<string, string>()
        {
            { "RFC", "RECEPTOR456" },
            { "Nombre", "Cliente Receptor" },
            { "Domicilio", "Avenida 456, Ciudad" }
        };

        List<Dictionary<string, string>> conceptos = new List<Dictionary<string, string>>()
        {
            new Dictionary<string, string>()
            {
                { "Descripcion", "Producto 1" },
                { "Cantidad", "2" },
                { "Precio", "100.00" }
            },
            new Dictionary<string, string>()
            {
                { "Descripcion", "Producto 2" },
                { "Cantidad", "1" },
                { "Precio", "50.00" }
            }
        };

        string version = "2";

        string xmlFactura = FacturaElectronicaMX.GenerarFactura(emisor, receptor, conceptos, version);
        Console.WriteLine(xmlFactura);