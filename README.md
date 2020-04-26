# Wpf-local-font-bug
A description to my problem of loading not installed font from a static-name file that changes and replaces programatically

# How to test?
build the program if needed (already built). run it and select exist fonts from comboBox, it works.
now delete a font file and paste another font with first's file name, program does not show font true (and callback to default) until restarting program. make a new instance of window not works too.
