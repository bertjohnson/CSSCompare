CSSCompare
==========

CSS Compare is a utility to compare two CSS files and output unique styles. It's useful for exporting customizations and managing stylesheet versions.

Unlike traditional text comparison tools, CSS Compare evaluates individual CSS styles instead of pure text blocks, allowing for net comparisons regardless of where a style may appear in the file. CSS Compare is compatible with all levels of CSS.

Sample Usage
============

`CSSCompare.exe -v1 C:\customized.css -v2 C:\original.css > C:\difference.css`

Background
==========

This project originated when I was working with a highly customized SharePoint 2007 farm. Its CSS files had been directly modified all over the place. Since SharePoint 2010 had new CSS files, I had to extract all styles that were tailored for that site.

This utility allowed me to easily export all of the customizations to one file and drop it into SharePoint 2010, which worked perfectly. I've since found a host of other uses for it.

Since it does style-by-style CSS comparisons instead of block-level text comparisons like other tools, it works better than pure text comparison tools.

License
=======

Copyright © 2012-2015 [Bert Johnson](https://bertjohnson.com)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
