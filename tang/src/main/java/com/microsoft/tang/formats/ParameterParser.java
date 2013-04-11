package com.microsoft.tang.formats;

import java.lang.reflect.Constructor;

import com.microsoft.tang.ExternalConstructor;
import com.microsoft.tang.exceptions.BindException;
import com.microsoft.tang.util.MonotonicMap;
import com.microsoft.tang.util.ReflectionUtilities;

public class ParameterParser {
  MonotonicMap<String, Constructor<? extends ExternalConstructor<?>>> parsers = new MonotonicMap<>();
 
  @SuppressWarnings({ "unchecked", "rawtypes" })
  public void addParser(Class<? extends ExternalConstructor<?>> ec) throws BindException {
    Class<?> tc = (Class<?>)ReflectionUtilities.getInterfaceTarget(
        ExternalConstructor.class, ec);
    addParser((Class)tc, (Class)ec);
  }
  public <T, U extends T> void addParser(Class<U> clazz, Class<? extends ExternalConstructor<T>> ec) throws BindException {
    Constructor<? extends ExternalConstructor<T>> c;
    try {
      c = ec.getConstructor(String.class);
    } catch (NoSuchMethodException e) {
      throw new BindException("Constructor "
          + ReflectionUtilities.getFullName(ec) + "(String) does not exist!", e);
    }
    c.setAccessible(true);
    parsers.put(ReflectionUtilities.getFullName(clazz), c);
  }

  public void mergeIn(ParameterParser p) {
    for (String s : p.parsers.keySet()) {
      if (!parsers.containsKey(s)) {
        parsers.put(s, p.parsers.get(s));
      } else {
        if (!parsers.get(s).equals(p.parsers.get(s))) {
          throw new IllegalArgumentException(
              "Conflict detected when merging parameter parsers! To parse " + s
                  + " I have a: " + ReflectionUtilities.getFullName(parsers.get(s).getDeclaringClass())
                  + " the other instance has a: " + ReflectionUtilities.getFullName(p.parsers.get(s).getDeclaringClass()));
        }
      }
    }
  }

  public <T> T parse(Class<T> c, String s) {
    Class<?> d = ReflectionUtilities.boxClass(c);
    String name = ReflectionUtilities.getFullName(d);
    return parse(name, s);
  }
  @SuppressWarnings("unchecked")
  public <T> T parse(String name, String value) {
    if (parsers.containsKey(name)) {
      try {
        return (T) (parsers.get(name).newInstance(value).newInstance());
      } catch (ReflectiveOperationException e) {
        throw new IllegalArgumentException("Error invoking constructor for "
            + name, e);
      }
    } else {
      if (name.equals(String.class.getName())) {
        return (T) value;
      }
      if (name.equals(Byte.class.getName())) {
        return (T) (Byte) Byte.parseByte(value);
      }
      if (name.equals(Character.class.getName())) {
        return (T) (Character) value.charAt(0);
      }
      if (name.equals(Short.class.getName())) {
        return (T) (Short) Short.parseShort(value);
      }
      if (name.equals(Integer.class.getName())) {
        return (T) (Integer) Integer.parseInt(value);
      }
      if (name.equals(Long.class.getName())) {
        return (T) (Long) Long.parseLong(value);
      }
      if (name.equals(Float.class.getName())) {
        return (T) (Float) Float.parseFloat(value);
      }
      if (name.equals(Double.class.getName())) {
        return (T) (Double) Double.parseDouble(value);
      }
      if (name.equals(Void.class.getName())) {
        throw new ClassCastException("Can't instantiate void");
      }
      throw new UnsupportedOperationException("Don't know how to parse a " + name);
    }
  }

}
