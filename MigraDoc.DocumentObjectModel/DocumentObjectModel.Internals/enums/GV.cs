#region MigraDoc - Creating Documents on the Fly
//
// Authors:
//   Stefan Lange
//   Klaus Potzesny
//   David Stephensen
//
// Copyright (c) 2001-2017 empira Software GmbH, Cologne Area (Germany)
//
// http://www.pdfsharp.com
// http://www.migradoc.com
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

// ReSharper disable InconsistentNaming

namespace MigraDoc.DocumentObjectModel.publics
{
    /// <summary>
    /// Indicates how to retrieve a value from a DocumentObject.
    /// </summary>
    public enum GV
    {
        /// <summary>
        /// Gets the value for reading and writing. If the value does not exist, it is created.
        /// </summary>
        ReadWrite = 0,

        /// <summary>
        /// Gets the value for reading. If the value does not exist, it is not created.
        /// </summary>
        ReadOnly = 1,

        /// <summary>
        /// Returns null if value is Null or does not exist.
        /// </summary>
        GetNull = 2,
    }
}
