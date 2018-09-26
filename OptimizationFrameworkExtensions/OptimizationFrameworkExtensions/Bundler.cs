// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Bundler.cs" company="EMCOR Group, Inc.">
//   Copyright © 2009-2016 EMCOR Group, Inc. All Rights Reserved. 
//   This software and source code is owned by EMCOR Group, 
//   Inc. and is protected by United States and International 
//   copyright laws and treaties, as well as other intellectual 
//   property laws and treaties, and may not be reproduced, 
//   distributed, transmitted, displayed, published or broadcast 
//   without the prior written permission of EMCOR Group, Inc.
//   You may not alter or remove any copyright or other notice 
//   from copies of the source code or software.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace EMCOR.Common.Web.Optimization {
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Optimization;
    using System.Web.UI.WebControls;

    using EMCOR.Common.Cryptography;

    /// <summary>
    /// The bundler.
    /// </summary>
    public static class Bundler {
        #region Fields and Constants

        /// <summary>
        /// The _context.
        /// </summary>
        #pragma warning disable 649
        private static HttpContextBase _context;
        #pragma warning restore 649

        #endregion

        #region Public Enums

        /// <summary>
        /// The bundle type.
        /// </summary>
        public enum BundleType {
            /// <summary>
            /// The generic.
            /// </summary>
            Generic, 

            /// <summary>
            /// The script.
            /// </summary>
            Script, 

            /// <summary>
            /// The style.
            /// </summary>
            Style
        }

        #endregion

        #region Properties and Indexers

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        private static HttpContextBase Context {
            get {
                return _context ?? new HttpContextWrapper(HttpContext.Current);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// The add bundles to place holder.
        /// </summary>
        /// <param name="bundlePaths">
        /// The bundle paths.
        /// </param>
        /// <param name="placeHolder">
        /// The place holder.
        /// </param>
        /// <param name="bundleType">
        /// The bundle type.
        /// </param>
        public static void AddBundlesToPlaceHolder(string[] bundlePaths, PlaceHolder placeHolder, BundleType bundleType = BundleType.Generic) {
            foreach (var bundlePath in bundlePaths) {
                AddBundleToPlaceHolder(bundlePath, placeHolder, bundleType);
            }
        }

        /// <summary>
        /// The add bundle to place holder.
        /// </summary>
        /// <param name="bundlePath">
        /// The bundle path.
        /// </param>
        /// <param name="placeHolder">
        /// The place holder.
        /// </param>
        /// <param name="bundleType">
        /// The bundle type.
        /// </param>
        public static void AddBundleToPlaceHolder(string bundlePath, PlaceHolder placeHolder, BundleType bundleType = BundleType.Generic) {
            var innerText = string.Empty;
            var bundleIsRegistered = BundleTable.Bundles.GetRegisteredBundles().Any(bundle => bundle.Path == bundlePath);

            if (!bundleIsRegistered) {
                throw new Exception(string.Format("Bundle has not been registered: {0}", bundlePath));
            }

            switch (bundleType) {
                case BundleType.Generic:
                    throw new Exception("Must be rendered as BundleType.Script or BundleType.Style");
                case BundleType.Script:
                    innerText = Scripts.Render(bundlePath).ToString();
                    break;
                case BundleType.Style:
                    innerText = Styles.Render(bundlePath).ToString();
                    break;
            }

            var newLiteral = new Literal { Text = innerText };

            var exists = placeHolder.Controls.Cast<object>().OfType<Literal>().Any(literal => literal.Text == newLiteral.Text);
            if (exists) {
                return;
            }

            placeHolder.Controls.Add(newLiteral);
        }

        /// <summary>
        /// The register bundle.
        /// </summary>
        /// <param name="bundlePath">
        /// The bundle path.
        /// </param>
        /// <param name="filePath">
        /// The file path.
        /// </param>
        /// <param name="bundleType">
        /// The bundle type.
        /// </param>
        public static void RegisterBundle(string bundlePath, string filePath, BundleType bundleType = BundleType.Generic) {
            RegisterBundle(bundlePath, new[] { filePath });
        }

        /// <summary>
        /// The register bundle.
        /// </summary>
        /// <param name="bundlePath">
        /// The bundle path.
        /// </param>
        /// <param name="filePaths">
        /// The file paths.
        /// </param>
        /// <param name="bundleType">
        /// The bundle type.
        /// </param>
        public static void RegisterBundle(string bundlePath, string[] filePaths, BundleType bundleType = BundleType.Generic) {
            var bundleIsRegistered = BundleTable.Bundles.GetRegisteredBundles().Any(bundle => bundle.Path == bundlePath);

            if (bundleIsRegistered) {
                return;
            }

            Bundle newBundle;
            switch (bundleType) {
                case BundleType.Script:
                    newBundle = new ScriptBundle(bundlePath);
                    newBundle.Include(filePaths);
                    break;

                case BundleType.Style:
                    newBundle = new StyleBundle(bundlePath);
                    foreach (var filePath in filePaths) {
                        newBundle.Include(filePath, new CssUrlRewriteVirtualPathTransform());
                    }

                    break;

                default:
                    newBundle = new Bundle(bundlePath);
                    newBundle.Include(filePaths);
                    break;
            }

            BundleTable.Bundles.Add(newBundle);
        }

        #endregion

        #region Nested Types

        /// <summary>
        /// The scripts.
        /// </summary>
        public static class Scripts {
            #region Public Methods

            /// <summary>
            /// The register bundle.
            /// </summary>
            /// <param name="bundlePath">
            /// The bundle path.
            /// </param>
            /// <param name="filePath">
            /// The file path.
            /// </param>
            public static void RegisterBundle(string bundlePath, string filePath) {
                RegisterBundle(bundlePath, new[] { filePath });
            }

            /// <summary>
            /// The register bundle.
            /// </summary>
            /// <param name="bundlePath">
            /// The bundle path.
            /// </param>
            /// <param name="filePaths">
            /// The file paths.
            /// </param>
            public static void RegisterBundle(string bundlePath, string[] filePaths) {
                Bundler.RegisterBundle(bundlePath, filePaths, BundleType.Script);
            }

            /// <summary>
            /// The render.
            /// </summary>
            /// <param name="paths">
            /// The paths.
            /// </param>
            /// <returns>
            /// The <see cref="IHtmlString"/>.
            /// </returns>
            public static IHtmlString Render(params string[] paths) {
                const string TagTemplate = "<script type=\"text/javascript\" src=\"{0}\"></script>";
                return RenderFormat(TagTemplate, paths);
            }

            /// <summary>
            /// The render format.
            /// </summary>
            /// <param name="format">
            /// The format.
            /// </param>
            /// <param name="paths">
            /// The paths.
            /// </param>
            /// <returns>
            /// The <see cref="IHtmlString"/>.
            /// </returns>
            public static IHtmlString RenderFormat(string format, params string[] paths) {
                var tagBuilder = new StringBuilder();
                tagBuilder.AppendLine();

                foreach (var path in paths) {
                    if (!BundleTable.EnableOptimizations) {
                        var bundle = BundleTable.Bundles.GetBundleFor(path);
                        var context = new BundleContext(Context, BundleTable.Bundles, path);
                        foreach (var url in from bundleFile in bundle.EnumerateFiles(context)
                            let filePath = context.HttpContext.Server.MapPath(bundleFile.IncludedVirtualPath)
                            let file = new FileInfo(filePath)
                            where file.Exists
                            let hash = EncryptionHandler.GetMd5Hash(string.Format("{0}|{1}|{2}", file.FullName, file.LastWriteTime, Config.BuildNumber))
                            select string.Format("{0}?v={1}", System.Web.Optimization.Scripts.Url(bundleFile.IncludedVirtualPath), hash)) {
                            tagBuilder.AppendFormat(format, url);
                            tagBuilder.AppendLine();
                        }
                    }
                    else {
                        tagBuilder.AppendFormat(format, System.Web.Optimization.Scripts.Url(path));
                        tagBuilder.AppendLine();
                    }
                }

                return new HtmlString(tagBuilder.ToString());
            }

            #endregion
        }

        /// <summary>
        /// The styles.
        /// </summary>
        public static class Styles {
            #region Public Methods

            /// <summary>
            /// The register bundle.
            /// </summary>
            /// <param name="bundlePath">
            /// The bundle path.
            /// </param>
            /// <param name="filePath">
            /// The file path.
            /// </param>
            public static void RegisterBundle(string bundlePath, string filePath) {
                RegisterBundle(bundlePath, new[] { filePath });
            }

            /// <summary>
            /// The register bundle.
            /// </summary>
            /// <param name="bundlePath">
            /// The bundle path.
            /// </param>
            /// <param name="filePaths">
            /// The file paths.
            /// </param>
            public static void RegisterBundle(string bundlePath, string[] filePaths) {
                Bundler.RegisterBundle(bundlePath, filePaths, BundleType.Style);
            }

            /// <summary>
            /// The render.
            /// </summary>
            /// <param name="paths">
            /// The paths.
            /// </param>
            /// <returns>
            /// The <see cref="IHtmlString"/>.
            /// </returns>
            public static IHtmlString Render(params string[] paths) {
                const string TagTemplate = "<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\"/>";
                return RenderFormat(TagTemplate, paths);
            }

            /// <summary>
            /// The render format.
            /// </summary>
            /// <param name="format">
            /// The format.
            /// </param>
            /// <param name="paths">
            /// The paths.
            /// </param>
            /// <returns>
            /// The <see cref="IHtmlString"/>.
            /// </returns>
            public static IHtmlString RenderFormat(string format, params string[] paths) {
                var tagBuilder = new StringBuilder();
                tagBuilder.AppendLine();

                foreach (var path in paths) {
                    if (!BundleTable.EnableOptimizations) {
                        var bundle = BundleTable.Bundles.GetBundleFor(path);
                        var context = new BundleContext(Context, BundleTable.Bundles, path);
                        foreach (var url in from bundleFile in bundle.EnumerateFiles(context)
                            let filePath = context.HttpContext.Server.MapPath(bundleFile.IncludedVirtualPath)
                            let file = new FileInfo(filePath)
                            where file.Exists
                            let hash = EncryptionHandler.GetMd5Hash(string.Format("{0}|{1}|{2}", file.FullName, file.LastWriteTime, Config.BuildNumber))
                            select string.Format("{0}?v={1}", System.Web.Optimization.Styles.Url(bundleFile.IncludedVirtualPath), hash)) {
                            tagBuilder.AppendFormat(format, url);
                            tagBuilder.AppendLine();
                        }
                    }
                    else {
                        tagBuilder.AppendFormat(format, System.Web.Optimization.Styles.Url(path));
                        tagBuilder.AppendLine();
                    }
                }

                return new HtmlString(tagBuilder.ToString());
            }

            #endregion
        }

        #endregion
    }
}