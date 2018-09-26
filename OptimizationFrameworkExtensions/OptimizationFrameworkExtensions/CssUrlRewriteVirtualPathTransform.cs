// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CssUrlRewriteVirtualPathTransform.cs" company="EMCOR Group, Inc.">
//   Copyright © 2009-2014 EMCOR Group, Inc. All Rights Reserved. 
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
    using System.Web;
    using System.Web.Optimization;

    /// <summary>
    /// The virtual path transform.
    /// </summary>
    public class CssUrlRewriteVirtualPathTransform : IItemTransform {
        #region Public Methods

        /// <summary>
        /// The process.
        /// </summary>
        /// <param name="includedVirtualPath">
        /// The included virtual path.
        /// </param>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string Process(string includedVirtualPath, string input) {
            var path = string.Format("~{0}", VirtualPathUtility.ToAbsolute(includedVirtualPath));
            return new CssRewriteUrlTransform().Process(path, input);
        }

        #endregion
    }
}