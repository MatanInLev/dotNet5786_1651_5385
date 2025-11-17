namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

internal class OrdersImplementation : IOrder
{
    /// <summary>
    /// Create a new order. Assigns a new Id, appends the order to the list,
    /// and saves the updated list to the XML file.
    /// </summary>
    public void Create(Order item)
    {
        // Load the current list from the XML file
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.order_file_name);

        // Get the next ID from the config
        int newId = Config.NextOrderId;
        var orderToAdd = item with { Id = newId };

        // Add the new item to the list
        orders.Add(orderToAdd);

        // Save the updated list back to the XML file
        XMLTools.SaveListToXMLSerializer(orders, Config.order_file_name);
    }

    /// <summary>
    /// Delete an order by id from the XML file.
    /// Throws <see cref="DalDoesNotExistException"/> when the id is not found.
    /// </summary>
    public void Delete(int id)
    {
        // Load the current list from XML
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.order_file_name);

        // Find the index of the item to delete
        int index = orders.FindIndex(o => o.Id == id);
        if (index == -1)
            throw new DalDoesNotExistException($"An object of type Order with ID: {id} does not exist.");

        // Remove the item
        orders.RemoveAt(index);

        // Save the updated list back to XML
        XMLTools.SaveListToXMLSerializer(orders, Config.order_file_name);
    }

    /// <summary>
    /// Remove all orders by saving an empty list to the XML file.
    /// </summary>
    public void DeleteAll()
    {
        // Create an empty list
        List<Order> emptyList = new List<Order>();

        // Save the empty list, overwriting the file
        XMLTools.SaveListToXMLSerializer(emptyList, Config.order_file_name);
    }

    /// <summary>
    /// Read an order by id from the XML file. Returns null when not found.
    /// </summary>
    public Order? Read(int id)
    {
        // Load the list from XML and find the item
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.order_file_name);
        return orders.FirstOrDefault(o => o.Id == id);
    }

    /// <summary>
    /// Read an order matching the given filter from the XML file. Returns null when not found.
    /// </summary>
    public Order? Read(Func<Order, bool> filter)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.order_file_name);
        return orders.FirstOrDefault(filter);
    }

    /// <summary>
    /// Read all orders from the XML file, optionally filtered by the given predicate.
    /// </summary>
    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.order_file_name);
        return filter == null ? orders : orders.Where(filter);
    }

    /// <summary>
    /// Update an existing order in the XML file.
    /// Throws <see cref="DalDoesNotExistException"/> when the id is not found.
    /// </summary>
    public void Update(Order item)
    {
        // Load the current list from XML
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.order_file_name);

        // Find the index of the item to update
        int index = orders.FindIndex(o => o.Id == item.Id);
        if (index == -1)
            throw new DalDoesNotExistException($"An object of type Order with ID: {item.Id} does not exist.");

        // Replace the old item with the new item
        orders[index] = item;

        // Save the updated list back to XML
        XMLTools.SaveListToXMLSerializer(orders, Config.order_file_name);
    }
}