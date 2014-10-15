function doIt(settings)
{
    // instead of returning primatives, it's easier to return documents... trust me!
    // in this case I chose to use 'Value' as the name because I'll be using a ValueHolder<string> to get the value out of the resutls
    return { Value: "Hello " + settings.name };
}

// <test>
var data = doIt({ name: 'World' });
print(data);
// </test>