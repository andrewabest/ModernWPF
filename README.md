ModernWPF
=========

A sample WPF application demonstrating modern approaches to common rich application requirements

## Bits and bobs included

* A sample Caliburn.Micro + Autofac integration, including messaging
* A custom Caliburn.Micro conductor implementation (LifetimeScopeConductor) that won't just resolve the items it conducts from the root lifetimescope
* An event aggregator subscription tracker that explicitly unsubscribes components when required (A Good Thing TM)
* A Caliburn.Micro-based dialog manager
* A modern approach to validation using Validar.Fody and PropertyChanged.Fody together with ComponentModel.DataAnnotations and INotifyDataErrorInfo
* A nice base validation template for controls
* A custom ICommand implementation (ActionBase) that can surface reasons why validation fails
* A nice toast-style alert system implemented with Caliburn.Micro
* An autogrid that works!
* A label that can reflect the validation requirements of the control it is bound to via reflection
* A simple focus behavior to focus the first field on a form
* A nice example of resilient teardown in a WPF application

* * * *

### A couple of other implementation points when creating a Caliburn.Micro-based WPF application

* Don't use Caliburn.Micro's IResult. Just Don't.
* Also don't use Caliburns action invoking conventions for allowing you to execute methods inline in the ViewModel as an action, except in the simplest of cases. If it has logic, or requires dependencies, it belongs in an ActionBase implementation.

