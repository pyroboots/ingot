namespace ingot.Generators;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        string schema = """
                        {
                          "$id": "3737626741",
                          "$schema": "http://json-schema.org/draft-07/schema#",
                          "description": "The entity_placer item component specifies the blocks that the item can be placed on.",
                          "properties": {
                            "dispense_on": {
                              "default": [],
                              "description": "List of block descriptors of the blocks that this item can be dispensed on. If left empty, all blocks will be allowed.",
                              "items": {
                                "oneOf": [
                                  {
                                    "properties": {
                                      "name": {
                                        "default": "",
                                        "type": "string"
                                      },
                                      "states": {
                                        "additionalProperties": {
                                          "oneOf": [
                                            {
                                              "type": "integer"
                                            },
                                            {
                                              "type": "string"
                                            },
                                            {
                                              "type": "boolean"
                                            }
                                          ],
                                          "title": "compound_proxy"
                                        },
                                        "default": {},
                                        "type": "object"
                                      },
                                      "tags": {
                                        "default": "",
                                        "type": "string"
                                      }
                                    },
                                    "title": "BlockDescriptorProxy",
                                    "type": "object",
                                    "x-runtime-constraint-description": "Check that a valid block name or expression is provided, and in the latter case verify that there are no block states as well"
                                  },
                                  {
                                    "minLength": 1,
                                    "type": "string"
                                  },
                                  {
                                    "properties": {
                                      "name": {
                                        "default": "",
                                        "type": "string"
                                      },
                                      "states": {
                                        "additionalProperties": {
                                          "oneOf": [
                                            {
                                              "type": "integer"
                                            },
                                            {
                                              "type": "string"
                                            },
                                            {
                                              "type": "boolean"
                                            }
                                          ],
                                          "title": "compound_proxy"
                                        },
                                        "default": {},
                                        "type": "object"
                                      },
                                      "tags": {
                                        "default": "",
                                        "type": "string"
                                      }
                                    },
                                    "title": "BlockDescriptorProxy",
                                    "type": "object",
                                    "x-runtime-constraint-description": "Check that a valid block name or expression is provided, and in the latter case verify that there are no block states as well"
                                  }
                                ],
                                "title": "Block Descriptor"
                              },
                              "type": "array"
                            },
                            "entity": {
                              "title": "Reference",
                              "type": "string",
                              "default": "",
                              "description": "The entity to be placed in the world.",
                              "minLength": 0,
                              "pattern": "^(?:\\w+(?:\\.\\w+)*:(?=\\w))?(?:\\w+(?:\\.\\w+)*)(?:<((?:\\w+(?:\\.\\w+)*:(?=\\w))?\\w+(?:\\.\\w+)*)*>)?$"
                            },
                            "use_on": {
                              "default": [],
                              "description": "List of block descriptors of the blocks that this item can be used on. If left empty, all blocks will be allowed.",
                              "items": {
                                "oneOf": [
                                  {
                                    "properties": {
                                      "name": {
                                        "default": "",
                                        "type": "string"
                                      },
                                      "states": {
                                        "additionalProperties": {
                                          "oneOf": [
                                            {
                                              "type": "integer"
                                            },
                                            {
                                              "type": "string"
                                            },
                                            {
                                              "type": "boolean"
                                            }
                                          ],
                                          "title": "compound_proxy"
                                        },
                                        "default": {},
                                        "type": "object"
                                      },
                                      "tags": {
                                        "default": "",
                                        "type": "string"
                                      }
                                    },
                                    "title": "BlockDescriptorProxy",
                                    "type": "object",
                                    "x-runtime-constraint-description": "Check that a valid block name or expression is provided, and in the latter case verify that there are no block states as well"
                                  },
                                  {
                                    "minLength": 1,
                                    "type": "string"
                                  },
                                  {
                                    "properties": {
                                      "name": {
                                        "default": "",
                                        "type": "string"
                                      },
                                      "states": {
                                        "additionalProperties": {
                                          "oneOf": [
                                            {
                                              "type": "integer"
                                            },
                                            {
                                              "type": "string"
                                            },
                                            {
                                              "type": "boolean"
                                            }
                                          ],
                                          "title": "compound_proxy"
                                        },
                                        "default": {},
                                        "type": "object"
                                      },
                                      "tags": {
                                        "default": "",
                                        "type": "string"
                                      }
                                    },
                                    "title": "BlockDescriptorProxy",
                                    "type": "object",
                                    "x-runtime-constraint-description": "Check that a valid block name or expression is provided, and in the latter case verify that there are no block states as well"
                                  }
                                ],
                                "title": "Block Descriptor"
                              },
                              "type": "array"
                            }
                          },
                          "title": "minecraft:entity_placer",
                          "type": "object",
                          "x-format-version": "1.20.50"
                        }
                        """;
        
        string iface = TraitGeneratorV2.GenerateItemFromSchema(schema, "ingot.Core.TraitSystem.Traits.Item");
        Console.WriteLine(iface);
    }
}