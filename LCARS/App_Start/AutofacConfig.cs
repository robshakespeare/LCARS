﻿using Autofac;
using Autofac.Integration.Mvc;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace LCARS
{
    public class AutofacConfig
    {
        public static void RegisterDependencies()
        {
            var credentials = GetCredentials();

            if (string.IsNullOrWhiteSpace(credentials.Key))
            {
                throw new Exception("You must specify Team City credentials. See repository Readme for details.");
            }

            var builder = new ContainerBuilder();

            // Register your MVC controllers.
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            builder.RegisterType<Domain.Common>().As<Domain.ICommon>();
            builder.RegisterType<Domain.Environments>().As<Domain.IEnvironments>();
            builder.RegisterType<Domain.Builds>().As<Domain.IBuilds>();

            builder.RegisterType<Repository.Environments>().As<Repository.IEnvironments>();
            builder.RegisterType<Repository.Common>().As<Repository.ICommon>();
            builder.RegisterType<Repository.Builds>()
                .As<Repository.IBuilds>()
                .WithParameter("username", credentials.Key)
                .WithParameter("password", credentials.Value);

            // Set the dependency resolver to be Autofac.
            var container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        // Cant put credentials on GitHub so segreated them into excluded XML file
        private static KeyValuePair<string, string> GetCredentials()
        {
            var credentialsFilePath = HttpContext.Current.Server.MapPath(@"~/App_Data/Creds.xml");

            if (!System.IO.File.Exists(credentialsFilePath))
            {
                return new KeyValuePair<string, string>();
            }

            var doc = XDocument.Load(credentialsFilePath);

            return new KeyValuePair<string, string>(doc.Root.Element("Username").Value, doc.Root.Element("Password").Value);
        }
    }
}