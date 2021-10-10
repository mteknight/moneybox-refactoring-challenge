**Please check steps and considerations at the end.**

# Moneybox Money Withdrawal

The solution contains a .NET core library (Moneybox.App) which is structured into the following 3 folders:

* Domain - this contains the domain models for a user and an account, and a notification service.
* Features - this contains two operations, one which is implemented (transfer money) and another which isn't (withdraw
  money)
* DataAccess - this contains a repository for retrieving and saving an account (and the nested user it belongs to)

## The task

The task is to implement a money withdrawal in the WithdrawMoney.Execute(...) method in the features folder. For
consistency, the logic should be the same as the TransferMoney.Execute(...) method i.e. notifications for low funds and
exceptions where the operation is not possible.

As part of this process however, you should look to refactor some of the code in the TransferMoney.Execute(...) method
into the domain models, and make these models less susceptible to misuse. We're looking to make our domain models rich
in behaviour and much more than just plain old objects, however we don't want any data persistance operations (i.e. data
access repositories) to bleed into our domain. This should simplify the task of implementing WithdrawMoney.Execute(...).

## Guidelines

* You should spend no more than 1 hour on this task, although there is no time limit
* You should fork or copy this repository into your own public repository (Github, BitBucket etc.) before you do your
  work
* Your solution must compile and run first time
* You should not alter the notification service or the the account repository interfaces
* You may add unit/integration tests using a test framework (and/or mocking framework) of your choice
* You may edit this README.md if you want to give more details around your work (e.g. why you have done something a
  particular way, or anything else you would look to do but didn't have time)

Once you have completed test, zip up your solution, excluding any build artifacts to reduce the size, and email it back
to our recruitment team.

Good luck!

## Steps

1. Add unit tests for the existing money transfers logic to avoid breaking it;
2. Refactor the validation logic for optimal quality and clean code, and allow reusability;
3. Implement the transfer withdrawal logic using the same validation logic from transfers.

## Considerations

- Since the existing code is not tested originally, it was not developed with TDD. However, the new withdrawal logic
  was.
- I added guards (defensive programming) to the ctor of the features as the dependencies are mandatory, based on the
  classes' logic. It is better to have a guard telling a dependency is missing than waiting for a NullReferenceException
  upon use.
- As it can be observed in the commit history, I did not do this assignment in one sitting and also made use of a few
  hours to complete it. I covered the existing logic with tests and refactored it towards the domain step by step.  
  I have implemented the Withdraw logic and copied important tests for the sake of completion, however, there are better
  ways to implement this given more time. This can be a good subject for us to discuss next time we speak.
- Touching slightly on the mention of making the domain more robust and safer, I believe there are some interesting
  approaches that can help on that matter. I did not implement any since it is outside the scope here, but we can
  certainly discuss some ideas.
