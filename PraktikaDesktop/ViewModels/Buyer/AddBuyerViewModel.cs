using PraktikaDesktop.Models;
using PraktikaDesktop.ViewModels.Dialog;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace PraktikaDesktop.ViewModels
{
    public class AddBuyerViewModel : ViewModelBase
    {
        #region Fields
        private HttpResponseMessage? _response;
        public readonly BuyerViewModel _buyerViewModel;
        public readonly SelectBuyerToSupplyDialogViewModel _selectBuyerViewModel;

        private Buyer _newBuyer = new();
        private bool _individualSelected;
        private bool _legalEntitySelected;
        #endregion Fields

        #region Properties
        public Buyer NewBuyer
        {
            get => _newBuyer;
            set => this.RaiseAndSetIfChanged(ref _newBuyer, value);
        }
        public bool IndividualSelected
        {
            get => _individualSelected;
            set => this.RaiseAndSetIfChanged(ref _individualSelected, value);
        }
        public bool LegalEntitySelected
        {
            get => _legalEntitySelected;
            set => this.RaiseAndSetIfChanged(ref _legalEntitySelected, value);
        }
        #endregion Properties

        #region Command
        public async void ClickSaveCommand()
        {
            if (_buyerViewModel != null)
            {
                if (IndividualSelected)
                {
                    if (NewBuyer.Individual.FullName.Length == 0 || NewBuyer.Individual.Phone.Length == 0 || NewBuyer.Individual.SeriesPassportNumber.Length == 0 || NewBuyer.Address.Length == 0)
                    {
                        InformationDialogViewModel informationDialogViewModel = new(_buyerViewModel.CurrentMainWindowViewModel, "Не все поля заполнены");
                        await _buyerViewModel.CurrentMainWindowViewModel.ShowDialog(informationDialogViewModel);
                    }
                    else
                    {
                        NewBuyer.LegalEntity = null;
                        _response = ApiRequest.Post("Buyer/InsertBuyer", NewBuyer);
                        _response = ApiRequest.Get("Buyer/GetBuyers");
                        NewBuyer = (_response.Content.ReadAsAsync<List<Buyer>>().Result).Last();

                        _buyerViewModel.GoBackCommand(false);
                        _buyerViewModel.SelectedSortType = "Физ. лица";
                        _buyerViewModel.RefreshList(NewBuyer.BuyerId);
                    }
                }
                else
                {
                    if (NewBuyer.LegalEntity.Organization.Length == 0 || NewBuyer.LegalEntity.Phone.Length == 0 || NewBuyer.LegalEntity.Bank.Length == 0 || NewBuyer.LegalEntity.CorrespondentAccount.Length == 0 ||
                        NewBuyer.LegalEntity.CheckingAccount.Length == 0 || NewBuyer.LegalEntity.Bic.Length == 0 || NewBuyer.LegalEntity.Rrc.Length == 0 || NewBuyer.LegalEntity.Tin.Length == 0 || NewBuyer.Address.Length == 0)
                    {
                        InformationDialogViewModel informationDialogViewModel = new(_buyerViewModel.CurrentMainWindowViewModel, "Не все поля заполнены");
                        await _buyerViewModel.CurrentMainWindowViewModel.ShowDialog(informationDialogViewModel);
                    }
                    else
                    {
                        NewBuyer.Individual = null;
                        _response = ApiRequest.Post("Buyer/InsertBuyer", NewBuyer);
                        _response = ApiRequest.Get("Buyer/GetBuyers");
                        NewBuyer = (_response.Content.ReadAsAsync<List<Buyer>>().Result).Last();

                        _buyerViewModel.GoBackCommand(false);
                        _buyerViewModel.SelectedSortType = "Юр. лица";
                        _buyerViewModel.RefreshList(NewBuyer.BuyerId);
                    }
                }
            }
            else
            {
                if (IndividualSelected)
                {
                    if (NewBuyer.Individual.FullName.Length == 0 || NewBuyer.Individual.Phone.Length == 0 || NewBuyer.Individual.SeriesPassportNumber.Length == 0 || NewBuyer.Address.Length == 0)
                    {
                        InformationDialogViewModel informationDialogViewModel = new(_selectBuyerViewModel.CurrentMainWindowViewModel, "Не все поля заполнены");
                        await _selectBuyerViewModel.CurrentMainWindowViewModel.ShowDialog(informationDialogViewModel);
                    }
                    else
                    {
                        NewBuyer.LegalEntity = null;
                        _response = ApiRequest.Post("Buyer/InsertBuyer", NewBuyer);
                        _response = ApiRequest.Get("Buyer/GetBuyers");
                        NewBuyer = (_response.Content.ReadAsAsync<List<Buyer>>().Result).Last();

                        _selectBuyerViewModel.GoBackCommand(false);
                        _selectBuyerViewModel.SelectedSortType = "Физ. лица";
                        _selectBuyerViewModel.RefreshList(NewBuyer.BuyerId);
                    }
                }
                else
                {
                    if (NewBuyer.LegalEntity.Organization.Length == 0 || NewBuyer.LegalEntity.Phone.Length == 0 || NewBuyer.LegalEntity.Bank.Length == 0 || NewBuyer.LegalEntity.CorrespondentAccount.Length == 0 ||
                        NewBuyer.LegalEntity.CheckingAccount.Length == 0 || NewBuyer.LegalEntity.Bic.Length == 0 || NewBuyer.LegalEntity.Rrc.Length == 0 || NewBuyer.LegalEntity.Tin.Length == 0 || NewBuyer.Address.Length == 0)
                    {
                        InformationDialogViewModel informationDialogViewModel = new(_selectBuyerViewModel.CurrentMainWindowViewModel, "Не все поля заполнены");
                        await _selectBuyerViewModel.CurrentMainWindowViewModel.ShowDialog(informationDialogViewModel);
                    }
                    else
                    {
                        NewBuyer.Individual = null;
                        _response = ApiRequest.Post("Buyer/InsertBuyer", NewBuyer);
                        _response = ApiRequest.Get("Buyer/GetBuyers");
                        NewBuyer = (_response.Content.ReadAsAsync<List<Buyer>>().Result).Last();

                        _selectBuyerViewModel.GoBackCommand(false);
                        _selectBuyerViewModel.SelectedSortType = "Юр. лица";
                        _selectBuyerViewModel.RefreshList(NewBuyer.BuyerId);
                    }
                }
            }
        }
        #endregion Command

        //Constructor
        public AddBuyerViewModel(BuyerViewModel buyerViewModel)
        {
            _buyerViewModel = buyerViewModel;
            NewBuyer.Individual = new();
            NewBuyer.LegalEntity = new();

            if (_buyerViewModel.SelectedSortType == "Физ. лица")
            {
                IndividualSelected = true;
                LegalEntitySelected = false;
            }
            else
            {
                LegalEntitySelected = true;
                IndividualSelected = false;
            }
        }
        public AddBuyerViewModel(SelectBuyerToSupplyDialogViewModel selectBuyerViewModel)
        {
            _selectBuyerViewModel = selectBuyerViewModel;
            NewBuyer.Individual = new();
            NewBuyer.LegalEntity = new();

            if (_selectBuyerViewModel.SelectedSortType == "Физ. лица")
            {
                IndividualSelected = true;
                LegalEntitySelected = false;
            }
            else
            {
                LegalEntitySelected = true;
                IndividualSelected = false;
            }
        }
    }
}
